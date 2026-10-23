# Testes executados e reprodução

Ambiente validado: Linux, SDK .NET 10.0.402, EF/Pomelo 9.0.0, MySQL 8.4.13 e Chromium. Testes usam `area_gourmet_validacao`, separado de `area_gourmet`. Não apagam bancos, tabelas ou registros: cada execução cria identificadores únicos e deixa dados fictícios rastreáveis no banco de validação.

## Resultados

- `dotnet restore`: aprovado.
- `dotnet tool restore`: aprovado, ferramenta 9.0.0 fixada no manifesto.
- `dotnet build --no-restore`: aprovado, zero erros e zero avisos.
- `dotnet ef migrations has-pending-model-changes --no-build`: nenhuma alteração pendente.
- Migração nova aplicada no banco local após backup/revisão e banco de validação preparado pelas cinco migrations; não houve remoção de dados.
- Suíte `tests/integracao.py`: **88 verificações aprovadas** por HTTP e consultas MySQL independentes, incluindo rollback e concorrência.
- Suíte `tests/navegador.cjs`: **12 verificações aprovadas** no Chromium: login, montagem, cliente/endereço, quantidade, filtros, responsividade, finalização via fetch/CSRF, detalhes e relatórios, sem exceções JavaScript.

Os resultados brutos da execução ficam em `docs/resultados-integracao.json` e `docs/resultados-navegador.json`. São verificações funcionais, não métricas de cobertura de código. Não há suíte xUnit neste projeto. Não foi descrito um zero-test run como aprovação.

## Falhas encontradas e corrigidas

A primeira execução da página de relatórios detectou uma projeção de record seguida de ordenação que o provedor MySQL não traduzia. A pivot agora usa projeção anônima no banco e mapeamento de resultado após a consulta. A suíte foi reiniciada e passou.

O cliente de inspeção MySQL inicialmente retornava um travessão em codificação local e interrompeu uma asserção. O runner passou a pedir `utf8mb4` explicitamente, sem alterar dados ou enfraquecer o teste. A execução final terminou com todas as 88 asserções.

## Reproduzir nesta nuvem

1. Inicie o MySQL local. Prepare **exclusivamente** o banco `area_gourmet_validacao`, dando acesso ao usuário de desenvolvimento.
2. Em um terminal, configure `ConnectionStrings__DefaultConnection` para esse banco, `ASPNETCORE_ENVIRONMENT=Development`, `DOTNET_ROLL_FORWARD=Major` e `ASPNETCORE_URLS=http://127.0.0.1:5175`.
3. Restaure ferramentas/pacotes, compile, revise/aplique migrations e crie o primeiro administrador de validação pelo comando do README. Guarde as credenciais **somente em arquivo fora do Git**, com permissão 0600. O JSON contém os campos `adminLogin`, `adminPassword`, `database` (igual a `area_gourmet_validacao`). Não use uma conta de produção.
4. Execute `dotnet run --no-build --no-launch-profile` nesse terminal. O servidor não deve receber as variáveis temporárias Bootstrap.
5. Em outro terminal, na raiz do projeto:

```bash
export AREA_TEST_CONFIG=/caminho/fora/do/repositorio/test-secrets.json
export AREA_TEST_URL=http://127.0.0.1:5175
export AREA_TEST_REPORT=/tmp/area-gourmet-tests.json
python3 tests/integracao.py
```

A inspeção SQL deste runner usa o contêiner local `area-gourmet-mysql` e sua variável de senha root existente, sem imprimir o valor. Portanto ele é específico desse ambiente Docker e requer Python 3/Docker CLI. Para outro banco, adapte o transporte da função `sql` mantendo as asserções e a proibição de apontar para produção.

Para o navegador, instale Playwright em diretório de testes fora do código da aplicação, se não estiver disponível, e use Chromium:

```bash
npm install --prefix .test-tools playwright
export PLAYWRIGHT_MODULE="$PWD/.test-tools/node_modules/playwright"
export CHROMIUM_PATH=/usr/bin/chromium
export AREA_BROWSER_OUTPUT=/tmp/area-gourmet-browser
node tests/navegador.cjs
```

O runner visual pressupõe dados de demonstração presentes (a suíte HTTP prepara isso) e saldos suficientes para uma marmita e um suco. Em Linux sem Chromium do sistema, instale o navegador pelo procedimento oficial do Playwright e ajuste `CHROMIUM_PATH`; não desative TLS ou verificações de pacotes. As capturas são evidência da execução local, não um preview público.

As credenciais de testes já preparadas nesta instância estão fora do checkout, sob `/workspace/.cloud-setup`, e não entram no ZIP ou nos commits.

## Limites de validação

Windows, deploy de produção, HTTPS externo, carga prolongada, backups/restauração em outra máquina e avaliação acadêmica presencial não foram executados. Nenhum teste usou pagamentos reais, dados pessoais reais ou envio de mensagens a terceiros.
