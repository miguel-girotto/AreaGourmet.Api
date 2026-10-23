# Entrega e estado verificado

Implementação feita no checkout original e enviada ao GitHub na branch `codex/area-gourmet-mvc-20261023`. A `main` foi preservada. O histórico anterior permanece; foram criados commits reais de diagnóstico, domínio/persistência, MVC, testes e correção de decimais.

Repositório: https://github.com/miguel-girotto/AreaGourmet.Api/tree/codex/area-gourmet-mvc-20261023

O envio da implementação foi confirmado pelo Git. Não foi criado nem mesclado pull request. A comprovação de uso e apresentação acadêmica do GitHub fica disponível no histórico; não se promete pontuação.

## Evidências

- Compilação final: zero erros/avisos.
- Restauração de pacotes e ferramenta EF: concluídas.
- Modelo e migration: nenhuma alteração de modelo pendente.
- 88 verificações HTTP/MySQL e 12 no Chromium aprovadas; resultados em JSON nesta pasta.
- Banco principal: migrations aplicadas, sem inserção dos usuários/pedidos fictícios de validação.
- Aplicação principal: página de login HTTP 200; rota de saúde retorna 401 para anônimos, como esperado.
- Banco de validação separado: credenciais aleatórias locais, fora dos commits e do ZIP; dados fictícios mantidos para rastreabilidade.
- Backup local anterior à evolução: fora do checkout. Não houve exclusão de banco/dados/migrations anteriores.

Para começar a demonstração no banco principal, escolha login/senha e execute o bootstrap seguro do README. Após entrar, gere os dados fictícios pelo painel se desejar. Não existe senha administrativa padrão.

As instruções reutilizáveis da nuvem (`install_script` e `start_skill`) foram salvas no rascunho, incluindo reinicialização de serviços, autenticação dos checks e revisão manual de migrations. Salvar o rascunho não publica o ambiente; revise/salve nas configurações e publique pela interface. Não foi validada restauração em uma nova tarefa após publicação.

O ZIP final é um arquivo do Git com o código e documentação, sem `.git`, binários, `obj`, backups, banco ou configurações secretas. As capturas de tela e o ZIP estão na pasta de artefatos da entrega, fora do checkout.
