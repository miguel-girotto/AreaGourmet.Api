"""Testes HTTP + MySQL reais, exclusivos do banco area_gourmet_validacao.
Credenciais lidas de AREA_TEST_CONFIG, nunca impressas. Python 3, sem bibliotecas externas.
O runner cria dados fictícios pelo HTTP e NÃO apaga bancos ou registros.
"""
import concurrent.futures
import copy
import html
import http.cookiejar
import json
import os
import re
import secrets
import subprocess
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid
from pathlib import Path

BASE = os.environ.get('AREA_TEST_URL', 'http://127.0.0.1:5175')
CONFIG = json.loads(Path(os.environ['AREA_TEST_CONFIG']).read_text())
assert CONFIG['database'] == 'area_gourmet_validacao', 'Use somente o banco de validação.'
RUN = uuid.uuid4().hex[:8]
RESULTS = []
class Client:
    def __init__(self):
        self.cookies = http.cookiejar.CookieJar()
        self.opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(self.cookies))
    def request(self, path, data=None, as_json=False, csrf=None):
        headers = {}
        if data is not None:
            headers['Content-Type'] = 'application/json' if as_json else 'application/x-www-form-urlencoded'
            data = (json.dumps(data) if as_json else urllib.parse.urlencode(data)).encode()
        if csrf: headers['X-CSRF-TOKEN'] = csrf
        try:
            r = self.opener.open(urllib.request.Request(BASE+path, data=data, headers=headers), timeout=30)
        except urllib.error.HTTPError as e: r = e
        return r.status, html.unescape(r.read().decode()), r.geturl()
    def token(self, path='/Vendas'):
        status, body, _ = self.request(path)
        assert status == 200, f'Token page {path}: {status}'
        match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body)
        assert match, 'CSRF token missing'
        return match.group(1)
    def post(self, path, data, token_path='/Vendas'):
        data = dict(data, __RequestVerificationToken=self.token(token_path))
        return self.request(path, data)
    def login(self, login, password):
        return self.post('/Account/Login', {'Login':login, 'Senha':password}, '/Account/Login')

def sql(query):
    r = subprocess.run(['docker', 'exec', '-i', 'area-gourmet-mysql', 'sh', '-c', 'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" mysql --default-character-set=utf8mb4 -uroot area_gourmet_validacao --batch --skip-column-names'], input=query, text=True, capture_output=True, check=True)
    return r.stdout.strip()
def num(query): return float(sql(query))
def check(name, condition):
    if not condition: raise AssertionError(name)
    RESULTS.append(name); print('PASS', name, flush=True)
def create(client, path, data):
    status, body, _ = client.post(path, data)
    assert status == 200 and 'validation-summary-errors' not in body, f'Create failed: {path}, HTTP {status}'
    return body

def sale(client, product, quantidade=1, **kw):
    form = {'chaveOperacao':str(uuid.uuid4()), 'tipoAtendimento':'Retirada','formaPagamento':'PIX','taxaEntrega':0,'finalizar':True,'itens':[{'produtoId':product,'quantidade':quantidade}]}
    form.update(kw)
    status, body, _ = client.request('/api/pedidos',form,True,client.token())
    return status, json.loads(body), form

anon = Client()
check('Página protegida redireciona para login', '/Account/Login' in anon.request('/Vendas')[2])
check('API anônima retorna 401', anon.request('/api/health/database')[0] == 401)
check('POST sem CSRF bloqueado', anon.request('/Account/Login', {'Login':'invalido','Senha':'invalida'})[0] == 400)
admin = Client()
check('Login inválido rejeitado sem detalhes sensíveis', 'Login ou senha inválidos' in admin.login(CONFIG['adminLogin'],'invalida')[1])
check('Login administrador válido', '/Vendas' in admin.login(CONFIG['adminLogin'], CONFIG['adminPassword'])[2])
check('Conexão MySQL autenticada', 'conectado' in admin.request('/api/health/database')[1])
check('OpenAPI autenticado', admin.request('/openapi/v1.json')[0] == 200)
for page in ['/Home','/Cadastros?tipo=Clientes','/Cadastros?tipo=Categorias','/Cadastros?tipo=Produtos','/Usuarios','/Estoque','/Estoque/Consumos','/Estoque/Historico','/Relatorios','/Relatorios/Auditoria','/Pedidos','/Vendas/Cardapio']:
    check('Página integrada '+page, admin.request(page)[0] == 200)
# Idempotent demonstration data generation.
admin.post('/Demo/Gerar', {})
counts = sql('SELECT CONCAT((SELECT COUNT(*) FROM Produtos),",",(SELECT COUNT(*) FROM Clientes),",",(SELECT COUNT(*) FROM Consumos));')
admin.post('/Demo/Gerar', {})
check('Carga demo repetida sem duplicar produtos/clientes/consumos', counts == sql('SELECT CONCAT((SELECT COUNT(*) FROM Produtos),",",(SELECT COUNT(*) FROM Clientes),",",(SELECT COUNT(*) FROM Consumos));'))
# Real CRUDs with filters and validation.
create(admin, '/Cadastros/Editar?tipo=Categorias', {'Nome':'Teste '+RUN, 'Ativo':'true'})
cat = int(sql("SELECT Id FROM Categorias WHERE Nome='Teste "+RUN+"';"))
check('Categoria persistida e filtro real', 'Teste '+RUN in admin.request('/Cadastros?tipo=Categorias&q='+RUN)[1])
status, body, _ = admin.post('/Cadastros/Editar?tipo=Produtos', {'Nome':'Produto inválido '+RUN,'CategoriaId':cat,'PrecoAtual':'-1','Ativo':'true'})
check('Preço negativo rejeitado no servidor', 'validation-summary-errors' in body and num("SELECT COUNT(*) FROM Produtos WHERE Nome='Produto inválido "+RUN+"';") == 0)
create(admin, '/Cadastros/Editar?tipo=Produtos', {'Nome':'Produto '+RUN,'CategoriaId':cat,'PrecoAtual':'12.50','Ativo':'true','DisponivelVenda':'true'})
prod = int(sql("SELECT Id FROM Produtos WHERE Nome='Produto "+RUN+"';"))
check('Produto persistido e filtrado por categoria', 'Produto '+RUN in admin.request('/Cadastros?tipo=Produtos&categoriaId='+str(cat)+'&q='+RUN)[1])
phone = '18'+str(int(RUN,16)).zfill(9)[-9:]
create(admin, '/Cadastros/Editar?tipo=Clientes', {'Nome':'Cliente '+RUN,'Telefone':phone,'Ativo':'true'})
cli = int(sql("SELECT Id FROM Clientes WHERE Nome='Cliente "+RUN+"';"))
check('Cliente persistido e pesquisado por telefone', 'Cliente '+RUN in admin.request('/Cadastros?tipo=Clientes&q='+phone)[1])
create(admin, '/Cadastros/Editar?tipo=Clientes', {'Id':cli,'Nome':'Cliente editado '+RUN,'Telefone':phone,'Ativo':'true'})
check('Edição cliente persistida', 'Cliente editado '+RUN in admin.request('/Cadastros/Detalhes?tipo=Clientes&id='+str(cli))[1])
for i in [1,2]: create(admin, '/Cadastros/Endereco', {'ClienteId':cli,'Logradouro':'Rua teste '+str(i),'Numero':str(i),'Bairro':'Centro','Rotulo':'Endereço '+str(i)})
addresses = [int(n) for n in sql('SELECT Id FROM Enderecos WHERE ClienteId='+str(cli)+' ORDER BY Id;').splitlines()]
check('Cliente com múltiplos endereços reais', len(addresses) == 2)
check('Busca rápida inclui endereços', len(json.loads(admin.request('/api/clientes?telefone='+phone)[1])[0]['enderecos']) == 2)
# Caixa created through protected UI.
caixa_login = 'caixa.'+RUN
caixa_password = secrets.token_urlsafe(24)
create(admin, '/Usuarios/Editar', {'Nome':'Caixa '+RUN,'Login':caixa_login,'Senha':caixa_password,'Perfil':'Caixa','Ativo':'true','PodeCadastrarClientes':'true'})
caixa = Client()
check('Login Caixa válido', '/Vendas' in caixa.login(caixa_login,caixa_password)[2])
for path in ['/Home','/Usuarios','/Relatorios','/Estoque','/Cadastros?tipo=Categorias','/Cadastros?tipo=Produtos','/Cadastros/Editar?tipo=Produtos']:
    check('Caixa bloqueado no acesso direto '+path, caixa.request(path)[0] == 403)
check('Caixa bloqueado em POST administrativo', caixa.post('/Cadastros/Editar?tipo=Categorias', {'Nome':'Ataque '+RUN,'Ativo':'true'})[0] == 403)
check('Caixa pode cadastrar cliente quando autorizado', caixa.request('/Cadastros/Editar?tipo=Clientes')[0] == 200)
adminid = int(sql("SELECT Id FROM Usuarios WHERE Login='"+CONFIG['adminLogin']+"';"))
status, body, _ = admin.post('/Usuarios/Editar', {'Id':adminid,'Nome':'Administrador','Login':CONFIG['adminLogin'],'Perfil':'Caixa','Ativo':'false','PodeCadastrarClientes':'true'})
check('Último administrador não pode ser desativado/rebaixado', 'último administrador' in body and num('SELECT Ativo FROM Usuarios WHERE Id='+str(adminid)) == 1)
check('Senhas armazenadas como hash', sql("SELECT LEFT(Senha,6) FROM Usuarios WHERE Login='"+caixa_login+"';").startswith('AQAAAA'))
# Stock initial entry and invalid stock exit.
create(admin, '/Estoque/Movimento', {'ProdutoId':prod,'Tipo':'ENTRADA','Quantidade':'10','QuantidadeMinima':'2','Observacao':'Entrada de teste'})
check('Entrada de estoque registrada', num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == 10)
status, body, _ = admin.post('/Estoque/Movimento', {'ProdutoId':prod,'Tipo':'SAIDA','Quantidade':'11','QuantidadeMinima':'2','Observacao':'Saída inválida'})
check('Saída acima do saldo bloqueada', 'Saída superior' in body and num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == 10)
create(admin, '/Estoque/Movimento', {'ProdutoId':prod,'Tipo':'SAIDA','Quantidade':'1','QuantidadeMinima':'2','Observacao':'Saída manual de teste'})
check('Saída manual persistida', num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == 9)
# Field validation and forged price ignored.
check('Pedido vazio rejeitado', sale(caixa,prod,itens=[])[0] == 400)
check('Quantidade negativa rejeitada', sale(caixa,prod,quantidade=-1)[0] == 400)
check('Mesa sem número rejeitada', sale(caixa,prod,tipoAtendimento='Mesa')[0] == 400)
check('Delivery sem cliente/endereço rejeitado', sale(caixa,prod,tipoAtendimento='Delivery')[0] == 400)
check('Pagamento inválido rejeitado', sale(caixa,prod,formaPagamento='INVALIDO')[0] == 400)
check('Taxa em Retirada rejeitada', sale(caixa,prod,taxaEntrega=5)[0] == 400)
check('Endereço de outro cliente rejeitado', sale(caixa,prod,tipoAtendimento='Delivery',clienteId=cli,enderecoId=int(sql('SELECT Id FROM Enderecos WHERE ClienteId <> '+str(cli)+' LIMIT 1;')))[0] == 400)
status, data, original = sale(caixa,prod,quantidade=2,tipoAtendimento='Mesa',numeroMesa=3,valorTotal=0,subtotal=0)
pedido = data.get('id')
check('Mesa finalizada com cálculo no servidor', status == 200 and num('SELECT ValorTotal FROM Pedidos WHERE Id='+str(pedido)) == 25)
check('Operador registrado no pedido', sql('SELECT u.Login FROM Pedidos p JOIN Usuarios u ON u.Id=p.UsuarioId WHERE p.Id='+str(pedido)) == caixa_login)
check('Baixa automática de estoque', num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == 7)
status2, body, _ = caixa.request('/api/pedidos',original,True,caixa.token())
check('Reenvio idempotente sem baixa duplicada', status2 == 200 and json.loads(body)['id'] == pedido and num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == 7)
create(admin,'/Cadastros/Editar?tipo=Produtos', {'Id':prod,'Nome':'Produto '+RUN,'CategoriaId':cat,'PrecoAtual':'15.00','Ativo':'true','DisponivelVenda':'true'})
check('Preço histórico preservado após alteração do cardápio', num('SELECT PrecoUnitario FROM ItensPedido WHERE PedidoId='+str(pedido)) == 12.5)
status, retirada, _ = sale(caixa,prod,formaPagamento='Dinheiro')
check('Retirada finalizada', status == 200)
status, delivery, _ = sale(caixa,prod,tipoAtendimento='Delivery',clienteId=cli,enderecoId=addresses[0],taxaEntrega=5,formaPagamento='Cartão')
check('Delivery com taxa calculada e endereço congelado', status == 200 and num('SELECT ValorTotal FROM Pedidos WHERE Id='+str(delivery['id'])) == 20 and 'Rua teste 1' in sql('SELECT EnderecoHistorico FROM Pedidos WHERE Id='+str(delivery['id'])))
check('Estoque insuficiente reverte pedido inteiro', sale(caixa,prod,quantidade=999)[0] == 400)
status, aberto, _ = sale(caixa,prod,finalizar=False)
check('Comanda aberta não baixa estoque', status == 200 and sql('SELECT Status FROM Pedidos WHERE Id='+str(aberto['id'])) == 'ABERTO' and num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == 5)
caixa.post('/Pedidos/Status', {'id':aberto['id'],'status':'EM PREPARO'})
check('Fluxo ABERTO para EM PREPARO', sql('SELECT Status FROM Pedidos WHERE Id='+str(aberto['id'])) == 'EM PREPARO')
caixa.post('/Pedidos/Status', {'id':aberto['id'],'status':'FINALIZADO'})
caixa.post('/Pedidos/Status', {'id':aberto['id'],'status':'FINALIZADO'})
check('Finalização repetida baixa uma única vez', num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == 4)
caixa.post('/Pedidos/Status', {'id':pedido,'status':'CANCELADO'})
check('Caixa não pode cancelar pedido finalizado', sql('SELECT Status FROM Pedidos WHERE Id='+str(pedido)) == 'FINALIZADO')
admin.post('/Pedidos/Status', {'id':pedido,'status':'CANCELADO'})
admin.post('/Pedidos/Status', {'id':pedido,'status':'CANCELADO'})
check('Cancelamento estorna uma única vez', num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == 6 and num('SELECT COUNT(*) FROM Movimentacoes WHERE PedidoId='+str(pedido)+" AND Tipo='ESTORNO';") == 1)
# Packaging association consumed and reversed from original movement.
meal = int(sql("SELECT Id FROM Produtos WHERE Nome='Marmita Executiva' LIMIT 1;"))
emb = int(sql("SELECT e.Id FROM EstoqueBebidas e JOIN Produtos p ON p.Id=e.ProdutoId WHERE p.Nome='Embalagem de marmita' LIMIT 1;"))
saldo = num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE Id='+str(emb))
status, package_order, _ = sale(caixa,meal,quantidade=2)
check('Consumo associado de embalagem', status == 200 and num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE Id='+str(emb)) == saldo-2)
admin.post('/Pedidos/Status', {'id':package_order['id'],'status':'CANCELADO'})
check('Estorno de consumo associado preservado', num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE Id='+str(emb)) == saldo)
# A failure after one stock mutation must roll back all related effects.
prod_stock = int(sql('SELECT Id FROM EstoqueBebidas WHERE ProdutoId='+str(prod)))
create(admin, '/Estoque/Consumos', {'ProdutoId':meal,'EstoqueBebidaId':prod_stock,'Quantidade':'9999'})
link = int(sql('SELECT Id FROM Consumos WHERE ProdutoId='+str(meal)+' AND EstoqueBebidaId='+str(prod_stock)))
before_count = num('SELECT COUNT(*) FROM Pedidos;')
before_package = num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE Id='+str(emb))
check('Falha de consumo reverte pedido e todas as baixas', sale(caixa,meal)[0] == 400 and num('SELECT COUNT(*) FROM Pedidos;') == before_count and num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE Id='+str(emb)) == before_package)
admin.post('/Estoque/RemoverConsumo', {'id':link})
# Simultaneous retries of the same operation produce exactly one order.
shared = {'chaveOperacao':str(uuid.uuid4()),'tipoAtendimento':'Retirada','formaPagamento':'PIX','taxaEntrega':0,'finalizar':True,'itens':[{'produtoId':prod,'quantidade':1}]}
retry_clients = [Client(), Client()]
for c in retry_clients:
    for cookie in admin.cookies: c.cookies.set_cookie(copy.copy(cookie))
tokens = [c.token() for c in retry_clients]
stock_before = num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod))
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    repeated = list(pool.map(lambda x: x[0].request('/api/pedidos',shared,True,x[1]),zip(retry_clients,tokens)))
check('Reenvio concorrente da mesma chave é idempotente', all(r[0] == 200 for r in repeated) and len({json.loads(r[1])['id'] for r in repeated}) == 1 and num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == stock_before-1)

# Concurrency: two clients, last single unit, only one successful checkout.
create(admin, '/Estoque/Movimento', {'ProdutoId':prod,'Tipo':'SAIDA','Quantidade':str(num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod))-1),'QuantidadeMinima':'0','Observacao':'Preparar concorrência'})
clients = [Client(), Client()]
for c in clients:
    for cookie in caixa.cookies: c.cookies.set_cookie(copy.copy(cookie))
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    results = list(pool.map(lambda c: sale(c,prod), clients))
check('Concorrência não permite estoque negativo', sorted(r[0] for r in results) == [200,400] and num('SELECT QuantidadeAtual FROM EstoqueBebidas WHERE ProdutoId='+str(prod)) == 0)
# DB-backed filters and report totals.
status, report, _ = admin.request('/Relatorios?produtoId='+str(prod))
expected = num('SELECT COALESCE(SUM(ValorTotal),0) FROM Pedidos WHERE Status="FINALIZADO" AND Id IN (SELECT PedidoId FROM ItensPedido WHERE ProdutoId='+str(prod)+');')
formatted = 'R$ '+f'{expected:,.2f}'.replace(',','X').replace('.',',').replace('X','.')
check('Relatório LINQ com totalização bate com o MySQL', status == 200 and re.search(r'id="report-total">([^<]+)',report).group(1).replace('\xa0',' ') == formatted)
check('Pivot real e agrupamento renderizados', 'id="pivot"' in report and 'Vendas por pagamento' in report and 'Delivery' in report)
check('Filtro de período invertido validado', 'período válido' in admin.request('/Relatorios?inicio=2026-10-02&fim=2026-10-01')[1])
check('Filtro sem resultados retorna total zero', 'R$ 0,00' in admin.request('/Relatorios?inicio=2000-01-01&fim=2000-01-02')[1])
check('Filtro de movimentos por produto e tipo', 'Saída manual de teste' in admin.request('/Estoque/Historico?tipo=SAIDA&produtoId='+str(prod))[1])
# Deactivate all three CRUD entities and preserve references.
for tipo,id in [('Clientes',cli),('Produtos',prod),('Categorias',cat)]:
    admin.post('/Cadastros/Desativar', {'tipo':tipo,'id':id})
    check('Desativação segura '+tipo, num(f'SELECT Ativo FROM {tipo} WHERE Id={id};') == 0)
    check('Filtro de inativos '+tipo, RUN in admin.request('/Cadastros?tipo='+tipo+'&situacao=inativos&q='+RUN)[1])
check('Produto inativo bloqueado em venda direta', sale(caixa,prod)[0] == 400)
check('Pedido histórico continua acessível após desativação', admin.request('/Pedidos/Detalhes/'+str(pedido))[0] == 200)
check('Auditoria persistida sem senha', num('SELECT COUNT(*) FROM Auditorias;') > 0 and caixa_password not in admin.request('/Relatorios/Auditoria')[1])
caixaid = int(sql("SELECT Id FROM Usuarios WHERE Login='"+caixa_login+"';"))
create(admin,'/Usuarios/Editar',{'Id':caixaid,'Nome':'Caixa '+RUN,'Login':caixa_login,'Perfil':'Caixa','Ativo':'true','PodeCadastrarClientes':'false'})
check('Alteração de permissão revoga sessão anterior', '/Account/Login' in caixa.request('/Vendas')[2])
caixa.login(caixa_login,caixa_password)
check('Permissão de cadastro de cliente revogada', caixa.request('/Cadastros/Editar?tipo=Clientes')[0] == 403)
create(admin,'/Usuarios/Editar',{'Id':caixaid,'Nome':'Caixa '+RUN,'Login':caixa_login,'Perfil':'Caixa','Ativo':'false','PodeCadastrarClientes':'false'})
check('Conta desativada perde sessão', '/Account/Login' in caixa.request('/Vendas')[2])
check('Conta inativa não autentica', 'Login ou senha inválidos' in caixa.login(caixa_login,caixa_password)[1])
check('Página inexistente retorna 404', admin.request('/pagina-inexistente')[0] == 404)
admin.post('/Account/Logout', {})
check('Logout encerra sessão', '/Account/Login' in admin.request('/Vendas')[2])
report_path = Path(os.environ.get('AREA_TEST_REPORT','/tmp/area-gourmet-tests.json'))
report_path.write_text(json.dumps({'passed':len(RESULTS),'run':RUN,'database':'area_gourmet_validacao','checks':RESULTS},ensure_ascii=False,indent=2))
print('TOTAL:',len(RESULTS),'verificações aprovadas. Relatório:',report_path)
