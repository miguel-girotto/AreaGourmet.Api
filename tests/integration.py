#!/usr/bin/env python3
"""Integration test: HTTP, authentication, authorization and persisted sale invariants.
Uses only Python stdlib, .NET 8 and the app's SQLite provider; never touches MySQL.
"""
import contextlib, http.cookiejar, json, os, re, shutil, socket, sqlite3, subprocess, tempfile, time, urllib.request, urllib.parse, urllib.error, uuid
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
DOTNET=os.environ.get('DOTNET') or shutil.which('dotnet')
if not DOTNET: raise SystemExit('Install .NET 8 or set DOTNET to its executable.')
class Client:
    def __init__(self,base):
        self.base=base; self.jar=http.cookiejar.CookieJar(); self.opener=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(self.jar))
    def request(self,path,data=None,token=None,raw=False):
        headers={}
        if raw:
            body=json.dumps(data).encode();headers['Content-Type']='application/json'
        elif data is not None: body=urllib.parse.urlencode(data).encode()
        else: body=None
        if token: headers['RequestVerificationToken']=token
        request=urllib.request.Request(self.base+path,data=body,headers=headers)
        try:
            with self.opener.open(request,timeout=20) as response:return response.status,response.read().decode(),response.url
        except urllib.error.HTTPError as e:return e.code,e.read().decode(),e.url
    def token(self,path='/Vendas'):
        status,html,_=self.request(path);assert status==200,(path,status,html[:300])
        match=re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',html)
        assert match,('No CSRF token',path)
        return match.group(1)
    def form(self,path,values,token_path=None):
        values=dict(values,__RequestVerificationToken=self.token(token_path or path))
        return self.request(path,values)
    def login(self,email,password):
        status,html,url=self.form('/Conta/Login',{'Email':email,'Senha':password})
        assert status==200 and '/Conta/Login' not in url,(status,url,html[:200])

def run():
    subprocess.run([DOTNET,'build','--nologo'],cwd=ROOT,check=True,stdout=subprocess.DEVNULL)
    with tempfile.TemporaryDirectory(prefix='gourmet-test-') as tmp:
        dbfile=Path(tmp)/'test.db';log=Path(tmp)/'server.log'
        with socket.socket() as s:s.bind(('127.0.0.1',0));port=s.getsockname()[1]
        env=os.environ.copy();env.update({'ASPNETCORE_ENVIRONMENT':'Development','Database__Provider':'Sqlite','ConnectionStrings__Default':f'Data Source={dbfile}','Seed__AdminPassword':'Integration123!','Seed__DemoData':'true'})
        with log.open('w') as output:
            process=subprocess.Popen([DOTNET,'run','--no-build','--urls',f'http://127.0.0.1:{port}'],cwd=ROOT,env=env,stdout=output,stderr=output)
            try:
                admin=Client(f'http://127.0.0.1:{port}')
                for _ in range(120):
                    if process.poll() is not None:raise AssertionError(log.read_text())
                    try:
                        if admin.request('/Conta/Login')[0]==200:break
                    except (OSError,urllib.error.URLError):pass
                    time.sleep(.25)
                else:raise AssertionError('Server startup timed out')
                def scalar(sql,args=()):
                    with sqlite3.connect(dbfile) as db:return db.execute(sql,args).fetchone()[0]
                anonymous=admin.request('/Relatorios');assert '/Conta/Login' in anonymous[2]
                admin.login('admin@areagourmet.local','Integration123!')
                for path in ['/Vendas','/Clientes','/Clientes/Editar','/Clientes/Editar/1','/Clientes/Endereco?clienteId=1','/Produtos','/Produtos/Editar','/Produtos/Editar/1','/Produtos/Estoque','/Usuarios','/Usuarios/Editar','/Relatorios','/Vendas/Recibo/1']:
                    status,body,_=admin.request(path);assert status==200,(path,status,body[:500])
                print('PASS: seed, login and all Razor views')
                initial_count=scalar('SELECT COUNT(*) FROM Pedidos');initial_stock=scalar('SELECT EstoqueInsumo FROM Produtos WHERE Id=1')
                token=admin.token()
                def sale(**changes):
                    data={'chaveOperacao':str(uuid.uuid4()),'tipoAtendimento':'Retirada','formatoPagamento':'Pix','itens':[{'produtoId':1,'quantidade':2}]};data.update(changes);return data
                invalid=sale(tipoAtendimento='Delivery',clienteId=1,enderecoId=999)
                assert admin.request('/Vendas/Finalizar',invalid,token,True)[0]==400
                invalid=sale(itens=[{'produtoId':1,'quantidade':1},{'produtoId':2,'quantidade':999}])
                assert admin.request('/Vendas/Finalizar',invalid,token,True)[0]==400
                assert scalar('SELECT COUNT(*) FROM Pedidos')==initial_count
                assert scalar('SELECT EstoqueInsumo FROM Produtos WHERE Id=1')==initial_stock
                assert admin.request('/Vendas/Finalizar',sale(),raw=True)[0]==400
                assert admin.request('/Vendas/Finalizar',sale(itens=[{'produtoId':1,'quantidade':0}]),token,True)[0]==400
                print('PASS: invalid delivery, stock rollback, invalid quantity and CSRF rejection')
                data=sale(tipoAtendimento='Delivery',clienteId=1,enderecoId=1,valorTotal=.01,usuarioId=999)
                code,body,_=admin.request('/Vendas/Finalizar',data,token,True);assert code==200,(code,body)
                result=json.loads(body);order=result['id'];assert abs(result['valorTotal']-42.8)<.001
                again=admin.request('/Vendas/Finalizar',data,token,True);assert json.loads(again[1])['id']==order
                assert scalar('SELECT COUNT(*) FROM Pedidos')==initial_count+1
                assert scalar('SELECT EstoqueInsumo FROM Produtos WHERE Id=1')==initial_stock-2
                assert scalar('SELECT UsuarioId FROM Pedidos WHERE Id=?',(order,))==1
                print('PASS: server prices, delivery fee, operator audit, stock deduction and idempotency')
                version=scalar('SELECT Versao FROM Produtos WHERE Id=1')
                status,body,url=admin.form('/Produtos/Editar/1',{'Id':1,'Versao':version,'Nome':'Marmita Executiva','Categoria':'Marmitas','PrecoUnitario':'29,90','EstoqueInsumo':initial_stock-2,'ControlaEstoque':'true','Ativo':'true','Imagem':'meal.svg'})
                assert status==200 and url.endswith('/Produtos'),(status,url,body[:200])
                assert float(scalar('SELECT PrecoUnitario FROM Produtos WHERE Id=1'))==29.9
                assert float(scalar('SELECT PrecoPraticado FROM ItensPedido WHERE PedidoId=?',(order,)))==18.9
                receipt=admin.request(f'/Vendas/Recibo/{order}');assert receipt[0]==200 and '18,90' in receipt[1]
                print('PASS: product price edit preserves historical receipt')
                status,body,url=admin.form('/Clientes/Editar/0',{'Id':0,'Nome':'Teste Integração','Telefone':'11999990000','Ativo':'true'},'/Clientes/Editar')
                assert status==200 and scalar("SELECT COUNT(*) FROM Clientes WHERE Telefone='11999990000'")==1,(status,url,body[:100])
                client_id=scalar("SELECT Id FROM Clientes WHERE Telefone='11999990000'")
                status,body,url=admin.form('/Clientes/Endereco',{'Id':0,'ClienteId':client_id,'Logradouro':'Rua Teste','Numero':'10','Bairro':'Centro','Cidade':'Assis','CEP':'19800000','Ativo':'true'},f'/Clientes/Endereco?clienteId={client_id}')
                assert status==200 and scalar('SELECT COUNT(*) FROM Enderecos WHERE ClienteId=?',(client_id,))==1,(status,url,body[:100])
                wrong_address=sale(tipoAtendimento='Delivery',clienteId=client_id,enderecoId=1)
                assert admin.request('/Vendas/Finalizar',wrong_address,token,True)[0]==400
                print('PASS: client and address creation; address ownership validation')
                status,body,url=admin.form('/Usuarios/Editar/0',{'Id':0,'Nome':'Caixa Teste','Email':'caixa@test.local','Senha':'CaixaTeste123!','Perfil':'Caixa','Ativo':'true'},'/Usuarios/Editar')
                assert status==200 and url.endswith('/Usuarios'),(status,url,body[:100])
                cashier=Client(admin.base);cashier.login('caixa@test.local','CaixaTeste123!')
                assert cashier.request('/Vendas')[0]==200
                for path in ['/Usuarios','/Relatorios','/Produtos/Estoque','/Produtos/Editar/1']:assert cashier.request(path)[0]==403,path
                assert cashier.request('/Produtos/Excluir/1',{'__RequestVerificationToken':cashier.token()})[0]==403
                assert cashier.request(f'/Vendas/Recibo/{order}')[0]==403
                print('PASS: user creation and role enforcement for GET/POST and receipt ownership')
                start=time.strftime('%Y-01-01');end=time.strftime('%Y-12-31')
                report=admin.request(f'/Relatorios?inicio={start}&fim={end}');assert report[0]==200 and 'Marmitas' in report[1] and 'Pix' in report[1]
                assert admin.request('/Relatorios?inicio=2026-11-10&fim=2026-11-01')[0]==200
                admin.form(f'/Clientes/Excluir/{client_id}',{},'/Clientes')
                assert scalar('SELECT Ativo FROM Clientes WHERE Id=?',(client_id,))==0
                cashier_id=scalar("SELECT Id FROM Usuarios WHERE Email='caixa@test.local'")
                admin.form(f'/Usuarios/Excluir/{cashier_id}',{},'/Usuarios')
                assert '/Conta/Login' in cashier.request('/Vendas')[2]
                print('PASS: report, soft deletion and active-session revocation')
                print('ALL INTEGRATION CHECKS PASSED')
            finally:
                process.terminate()
                with contextlib.suppress(subprocess.TimeoutExpired):process.wait(timeout=10)
                if process.poll() is None:process.kill()
                if os.environ.get('KEEP_TEST_LOG'):print(log.read_text())
if __name__=='__main__':run()
