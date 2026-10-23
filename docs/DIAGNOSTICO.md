# Diagnóstico inicial e requisitos

Fontes lidas integralmente: pré-projeto Area Gourmet (3 páginas), trabalho final Frameworks (6 páginas), pedido do aluno e imagem de referência. O PDF acadêmico define 12 critérios, 10 pontos e apresentação até 27/11/2026. A imagem orienta o visual; o pedido define o trabalho de implementação.

Base: ASP.NET Core net10.0, OpenAPI 10.0.12, EF Tools 9.0.0 e Pomelo 9.0.0 (EF 9). Não há motivo para trocar pacotes. Há nove entidades, quatro migrations e um endpoint de conexão. Não existem controllers, views, autenticação ou testes. Banco MySQL 8.4 local acessível. Credenciais fora do Git. Checkout inicialmente limpo. Backup anterior à evolução salvo fora do checkout.

Plano incremental: evoluir entidades sem remover colunas; criar movimento e auditoria; gerar e revisar nova migration; implementar cookies, perfis e bootstrap seguro; CRUDs e filtros; serviço transacional de pedidos/estoque; consultas LINQ e pivot; Razor/CSS/JS; testes HTTP integrados e documentação.

| Critério acadêmico | Pontos | Entrega prevista |
|---|---:|---|
| Três CRUDs | 1,0 | Clientes, Produtos, Categorias; detalhes/edição/desativação |
| CSS | 0,5 | Layout responsivo verde inspirado na referência |
| Movimento | 1,0 | Entradas, saídas, vendas e estornos persistidos |
| Validação | 1,0 | View models e serviço de domínio; navegador |
| Principal/menu | 0,5 | Painel e navegação por perfil |
| Autenticação/autorização | 1,0 | Cookie, hash, CSRF e políticas no servidor |
| Geração de dados | 1,0 | Ação administrativa repetível e transacional |
| Filtros CRUD | 1,0 | Consultas EF filtradas no servidor |
| Múltiplas tabelas/totalização | 1,0 | Pedidos/operador/cliente/produtos e somas |
| Agrupamento | 0,5 | Pagamentos e produtos |
| Pivot | 0,5 | Datas × Mesa/Retirada/Delivery |
| GitHub | 1,0 | Commits incrementais; envio remoto depende de execução real |

Regras a verificar: perfis Administrador/Caixa, último administrador protegido, clientes com múltiplos endereços, modalidades com validação, preços históricos, idempotência, baixa/estorno sem duplicação, relações de consumo de embalagens, transações, relatórios excluindo cancelados e dados de demonstração sem senha fixa.
