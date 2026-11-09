# Área Gourmet — Sistema POS

Aplicação acadêmica ASP.NET Core MVC (.NET 8), Entity Framework Core e MySQL 8. Interface em português inspirada na referência enviada, com sidebar verde, catálogo e comanda lateral. Inclui fotografias ilustrativas locais e identidade visual com referência à logo do restaurante. Créditos e licenças em `docs/FOTOGRAFIAS.md`.

## Requisitos

- .NET SDK 8 e MySQL 8 (ou Docker Compose para iniciar apenas o banco).
- Python 3 para o teste de integração opcional.
- As dependências NuGet precisam ser restauradas na primeira execução.

## Executar com MySQL

1. Extraia o ZIP. Na pasta do projeto, copie `.env.example` para `.env` e altere as duas senhas.
2. Execute `docker compose up -d db` (ou crie um banco e usuário equivalentes em seu MySQL).
3. Configure as variáveis abaixo no mesmo terminal que executará a aplicação. A senha de conexão deve corresponder à de `.env`.

Linux/macOS:

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__Default='Server=localhost;Port=3306;Database=area_gourmet;User=gourmet;Password=SUA_SENHA_MYSQL'
export Seed__AdminEmail='admin@areagourmet.local'
export Seed__AdminPassword='ESCOLHA_UMA_SENHA_FORTE'
export Seed__DemoData=true
dotnet restore
dotnet run --urls http://localhost:5080
```

PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ConnectionStrings__Default='Server=localhost;Port=3306;Database=area_gourmet;User=gourmet;Password=SUA_SENHA_MYSQL'
$env:Seed__AdminEmail='admin@areagourmet.local'
$env:Seed__AdminPassword='ESCOLHA_UMA_SENHA_FORTE'
$env:Seed__DemoData='true'
dotnet restore
dotnet run --urls http://localhost:5080
```

Abra http://localhost:5080 e entre com o e-mail e senha configurados. A senha inicial exige pelo menos 10 caracteres. Ela é usada somente quando não há usuários no banco. Para criar caixas, entre em Usuários como administrador. Não existem senhas fixas no código.

O aplicativo aplica as migrations MySQL na inicialização. `database/schema.sql` contém o SQL gerado pelo EF para consulta ou instalação manual. Não aplique o SQL manualmente e as migrations sobre o mesmo banco sem controlar a tabela de histórico.

`Seed__DemoData=true` cria 16 produtos, uma cliente com endereço e doze vendas distribuídas em quatro meses. O seed só popula demonstração quando não há produtos e desconta as unidades das vendas geradas. Para uma base real, deixe `Seed__DemoData=false`.

## Demonstração rápida sem MySQL

SQLite é uma opção auxiliar de demonstração e testes. Não substitui a validação da instalação MySQL.

```bash
export ASPNETCORE_ENVIRONMENT=Development
export Database__Provider=Sqlite
export ConnectionStrings__Default='Data Source=gourmet-demo.db'
export Seed__AdminPassword='ESCOLHA_UMA_SENHA_FORTE'
export Seed__DemoData=true
dotnet run --urls http://localhost:5080
```

No PowerShell, use a sintaxe `$env:NOME='valor'`. SQLite usa `EnsureCreated`; migrations e SQL entregues são específicos de MySQL. Não alterne provedores mantendo a mesma conexão.

## Funcionalidades e regras

- Login por cookie HttpOnly, hash PBKDF2 do ASP.NET Core, proteção antiforgery nos POSTs e autorização verificada no servidor.
- Administrador: vendas, clientes, produtos, estoque, usuários e relatórios. Caixa: vendas, consulta do cardápio, cadastro/edição de clientes e endereços. Inativação de clientes e manutenção administrativa exigem Administrador.
- Busca de clientes por pelo menos três dígitos do telefone, seleção entre múltiplos endereços, mesa de 1 a 999 e três meios de pagamento.
- Mesa exige número; retirada permite cliente opcional; delivery exige cliente e endereço ativo pertencente a ele. Trocar o canal remove mesa/endereço não aplicáveis do pedido salvo.
- Delivery tem taxa fixa de R$ 5,00, calculada no servidor. Não há integração com cobrança real.
- Estoque simples por unidade vendável. Bebidas descontam unidades; marmitas podem representar porções ou kits de embalagem atribuídos ao próprio produto. Não há ficha técnica de ingredientes nem embalagem compartilhada entre produtos.
- Venda em transação serializável, validação de estoque e concorrência otimista por versão do produto. Falhas não devem registrar pedido parcial nem baixar parcialmente o estoque.
- UUID único por operação evita duplicar pedido ao reenviar uma finalização; o número da comanda só é atribuído ao salvar.
- Preço, nome e categoria ficam congelados nos itens; endereço de entrega fica copiado no pedido. Alterações posteriores no cadastro não alteram esses dados históricos.
- Exclusão lógica por inativação nos três CRUDs, mantendo referências históricas. Formulários permitem reativação. Usuário não pode inativar a si mesmo ou remover seu próprio perfil de administrador.
- Mudança de senha/inativação invalida sessões; alterações de perfil são verificadas a cada requisição autenticada.
- Relatório com período inclusivo em horário de Brasília, total de produtos, entrega, receita, ticket médio, operador e cliente; agrupamento por pagamento; pivot de categoria por mês com totais. Pivot exclui entrega e usa categorias históricas.
- Datas são armazenadas em UTC e exibidas em America/Sao_Paulo.
- Filtros nos CRUDs. Listas limitadas aos primeiros 200 resultados; use buscas para refinar.

## Estrutura em camadas

- `Models/`: entidades e validações.
- `Data/`: contexto, relacionamentos e geração de dados.
- `Services/`: operação transacional de venda.
- `ViewModels/`: contratos de entrada, formulários e relatório.
- `Controllers/`: autenticação, autorização, consultas e orquestração.
- `Views/`: Razor MVC.
- `wwwroot/`: CSS responsivo, JavaScript e fotografias locais.
- `Migrations/`: migração inicial MySQL e snapshot.
- `database/`: script SQL gerado.
- `tests/`: teste de integração HTTP + persistência SQLite.
- `docs/`: checklist acadêmico, modelo e validação.

A separação é por pastas dentro de um projeto, adequada à apresentação acadêmica; não é uma solução com vários projetos de Clean Architecture.

## Verificar

```bash
dotnet build
python3 tests/integration.py
```

O teste localiza `dotnet` no PATH ou na variável `DOTNET`, inicia sua própria instância em uma porta livre com um banco SQLite temporário e verifica os fluxos descritos em `docs/VALIDACAO.md`.

Para inspecionar/regenerar migrations e SQL:

```bash
dotnet tool restore
dotnet ef migrations list
dotnet ef migrations script --idempotent --output database/schema.sql
```

Para publicar arquivos de execução:

```bash
dotnet publish -c Release -o artifacts/publish
```

## GitHub durante o desenvolvimento

O requisito acadêmico exige uso durante o desenvolvimento. Este pacote não simula commits anteriores nem publica um repositório automaticamente. Inicie um repositório seu e mantenha commits reais das próximas alterações e validações:

```bash
git init
git add .
git commit -m "Adiciona base do sistema POS"
git branch -M main
git remote add origin URL_DO_SEU_REPOSITORIO
git push -u origin main
```

Não envie `.env`, senhas, bancos locais ou dados reais. Todos os membros devem entender e apresentar o que desenvolveram; a nota é individual.

## Instalação de produção

Este é um projeto acadêmico funcional, sem certificação de prontidão para produção. Configure HTTPS, segredos externos, backups, logs e monitoramento; use usuário MySQL com permissões adequadas. Aplique migrations como etapa controlada de implantação em ambientes com múltiplas réplicas. Adicione limitação de tentativas de login, recuperação de senha, auditoria de ajustes de estoque e testes de carga antes de uso real. Em produção a aplicação redireciona para HTTPS; o modo Development dos exemplos destina-se à execução local.

Fontes externas opcionais: Google Fonts para DM Sans, com fallback do sistema quando offline. Nenhuma biblioteca JavaScript externa é necessária.

## Atualização visual e cardápio demonstrativo

A interface usa vermelho e amarelo da logo pública do Área Gourmet, fundo claro e navegação escura. Os dados de contato e o endereço ficam na seção `Restaurante` do `appsettings.json`. Não foram informados horários de abertura e fechamento: a interface diz apenas "De domingo a domingo".

- Fotos são ilustrativas, não representam os pratos oficiais do estabelecimento. Consulte os créditos e as licenças individuais em `wwwroot/images/CREDITOS.txt`.
- A tela `/Restaurante` reúne logo, endereço e links de Instagram, WhatsApp e mapa. Links abrem os serviços; nenhuma mensagem é enviada automaticamente.
- A busca do painel funciona junto com as categorias e ignora diferenças de acento e caixa.
- Bancos existentes continuam funcionando, sem mudança de esquema ou necessidade de excluir o banco. Valores antigos de Imagem, como `meal.svg`, são resolvidos para as fotografias na interface.
- Para adicionar os nove exemplos novos ao banco que já tinha sete produtos, entre como Administrador, abra **Cardápio** e clique em **Adicionar exemplos**. A rotina considera o nome, inclui apenas os exemplos ausentes, mantém produtos personalizados e não altera preço, estoque ou pedidos existentes. Produtos inativos não são reativados. Repetir a ação não duplica os mesmos exemplos.
- Novas bases com `Seed__DemoData=true` já recebem os 16 exemplos.
- `Restaurante:CardapioDemonstrativo` é `true` por padrão e sinaliza os preços de exemplo. Só altere para `false` depois de cadastrar um cardápio real validado; as fotos continuam ilustrativas.
- Para mudar uma foto, edite o produto e escolha uma opção da lista. Os arquivos estão em `wwwroot/images/photos`. A entrada continua limitada a uma lista conhecida de arquivos locais.

Os produtos e preços são exemplos acadêmicos; o telefone e endereço de contato não implicam que o restaurante comercialize essas opções ou por esses valores.
