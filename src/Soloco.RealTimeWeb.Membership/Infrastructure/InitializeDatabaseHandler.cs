using System.Threading.Tasks;
using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Soloco.RealTimeWeb.Common;
using Soloco.RealTimeWeb.Common.Messages;
using Soloco.RealTimeWeb.Common.Store;
using Soloco.RealTimeWeb.Membership.Clients.Domain;
using Soloco.RealTimeWeb.Membership.Messages.Infrastructure;
using Soloco.RealTimeWeb.Membership.Users.Domain;

namespace Soloco.RealTimeWeb.Membership.Infrastructure
{
    public class InitializeDatabaseHandler : CommandHandler<InitializeDatabaseCommand>
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public InitializeDatabaseHandler(IDocumentSession session, UserManager<User> userManager, IConfiguration configuration)
            : base(session)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        protected override async Task<Result> Execute(InitializeDatabaseCommand command)
        {
            UpdateClients();

            await InitializeUsers();

            return Result.Success;
        }

        private async Task InitializeUsers()
        {
            if (await _userManager.FindByEmailAsync("tim@soloco.be") == null)
            {
                var user = new User("123456", "123 456", "tim@soloco.be");
                await _userManager.CreateAsync(user, "Aa-123456");
            }
        }

        private void UpdateClients()
        {
            var clients = Clients.Domain.Clients.Get(_configuration);
            foreach (var client in clients)
            {
                EnsureClient(client);
            }
        }

        private void EnsureClient(Client client)
        {
            var existing = Session.GetFirst<Client>(criteria => criteria.Key == client.Key);
            if (existing == null)
            {
                Session.Store(client);
            }
            else
            {
                UpdateClient(client, existing);
                Session.Store(existing);
            }
        }

        private static void UpdateClient(Client client, Client existing)
        {
            existing.Secret = client.Secret;
            existing.Name = client.Name;
            existing.ApplicationType = client.ApplicationType;
            existing.RedirectUri = client.RedirectUri;
            existing.Active = client.Active;
            existing.AllowedOrigin = client.AllowedOrigin;
        }
    }
}