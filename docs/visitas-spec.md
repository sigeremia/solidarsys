# Spec driven design — Visitas

## Objetivo
Implementar o módulo de gestão de visitas com o mesmo padrão visual e funcional dos CRUDs já existentes no projeto, respeitando a estrutura do backend e do frontend.

## Requisitos funcionais atendidos
- Lista de visitas por beneficiário com filtro por texto.
- Cadastro de visita com beneficiário, data, responsável e observações.
- Edição de visitas existentes.
- Exclusão de registros.
- Impedimento de duplicidade por beneficiário + data.

## Relacionamentos
- `Visita` pertence a `Beneficiario` por `id_beneficiario`.
- `Beneficiario` permanece a entidade principal de referência para o atendimento.
- A tabela de visitas não cria nova chave para usuário responsável; o campo responsável é mantido como texto conforme o protótipo.

## Backend
- Entidade: `Visita` em `SolidarSYS.Domain/Entities/Visita.cs`.
- DTOs e contratos em `SolidarSYS.Application/Visitas`.
- Repositório e serviço com consulta paginada, busca, cadastro, edição e exclusão.
- Controller: `api/visitas`.
- Banco: `visitas` com índice único em `(id_beneficiario, data_visita)`.

## Frontend
- Rota: `/visitas`.
- Tela principal: lista com busca, paginação e ações.
- Modal de cadastro/edição com seleção de beneficiário, data, responsável e observações.
- Estilo usando o mesmo padrão de classes e componentes PrimeNG já aplicados ao projeto.

## Etapas de implementação
1. Estruturar domínio e persistência.
2. Implementar API e regras de negócio.
3. Construir tela de listagem e formulário.
4. Validar fluxo com testes e build.
