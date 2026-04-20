# Desk Class - Sistema de Gerenciamento de Treinamentos

Este é um sistema web profissional para gerenciar turmas de treinamentos, baseado em uma planilha Excel existente. O sistema permite acesso remoto para várias pessoas com diferentes permissões.

## Funcionalidades

- **Gerenciamento de Turmas**: CRUD completo com status automático (Concluída, Em andamento, Futura)
- **Gerenciamento de Instrutores**: Cadastro e associação de instrutores às turmas
- **Gerenciamento de Clientes**: Base de dados de clientes participantes
- **Gerenciamento de Softwares**: Cadastro dos softwares disponíveis para treinamentos
- **Integração com Microsoft Teams**: Criação automática de reuniões online
- **Envio de Emails**: Notificações automáticas para instrutores e convites para clientes
- **Sistema de Autenticação**: Login/registro de usuários com roles (Admin, Instrutor, Participante)
- **Dashboard**: Estatísticas rápidas e interface profissional

## Estrutura dos Dados

### Turma
- Status (calculado automaticamente baseado nas datas)
- ID Autodesk
- Cliente (relacionamento obrigatório)
- Software (relacionamento obrigatório)
- Carga Horária
- Dias da Semana
- Data de Início/Fim
- Instrutor responsável
- Link da reunião Teams

### Cliente
- Nome
- Email
- Telefone
- Empresa

### Software
- Nome
- Descrição
- Versão

### Instrutor
- Nome
- Email

## Interface e Design

- **Paleta de Cores**: Azul profissional (#007bff), Verde para sucesso, Amarelo para andamento, Vermelho para alertas
- **Ícones Font Awesome**: Interface intuitiva com ícones
- **Bootstrap 5**: Design responsivo e moderno
- **Cards e Badges**: Visual organizado e informativo

## Como Executar

1. **Pré-requisitos**:
   - .NET 8.0 SDK
   - SQLite (incluído automaticamente)

2. **Configuração**:
   - Execute `dotnet restore` para instalar dependências
   - Execute `dotnet ef database update` para criar o banco de dados
   - Execute `dotnet run` para iniciar o servidor

3. **Acesso**:
   - Abra http://localhost:5245
   - Registre-se como usuário
   - Adicione clientes, softwares e instrutores primeiro
   - Crie turmas selecionando das listas disponíveis

## Funcionalidades Automáticas

- **Status da Turma**: Calculado automaticamente baseado na data atual
  - Verde: Concluída
  - Amarelo: Em andamento
  - Azul: Futura

- **Integração Teams**: Link gerado automaticamente ao criar turma

- **Emails**: Envio automático para instrutores quando turmas são criadas

## Desenvolvimento

O projeto usa:
- ASP.NET Core 8.0 com Razor Pages
- Entity Framework Core com SQLite
- Microsoft Graph SDK
- MailKit para emails
- Bootstrap 5 + Font Awesome
- Identity para autenticação

## Próximos Passos

- Configurar autenticação real com Azure AD
- Implementar importação de dados da planilha Excel
- Adicionar filtros avançados e relatórios
- Melhorar notificações e lembretes
- API REST para integrações externas