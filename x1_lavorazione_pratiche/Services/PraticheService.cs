using x1_lavorazione_pratiche.Common;
using x1_lavorazione_pratiche.DB.Contexts;
using x1_lavorazione_pratiche.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace x1_lavorazione_pratiche.Services
{
    public class PraticheService
    {
        private DBContext dbContext;
        public PraticheService(DBContext dBContext)
        {
            this.dbContext = dBContext;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> ChangePraticaStatus(PraticaModel praticaModel)
        {
            var pratica = this.dbContext.Pratiche
                     .Where(p => p.Id == praticaModel.Id)
                     .FirstOrDefault();
            if (pratica != null)
            {
                pratica.Status = praticaModel.Status;
                this.dbContext.PraticheHistory.Add(new DB.Entities.PraticaHistory()
                {
                    Id = Guid.NewGuid(),
                    HistoryTimestamp = DateTime.UtcNow,
                    PraticaId = pratica.Id,
                    CreatorName = pratica.CreatorName,
                    CreatorLastName = pratica.CreatorLastName,
                    CreatorDateOfBirth = pratica.CreatorDateOfBirth.HasValue ? DateTime.SpecifyKind(pratica.CreatorDateOfBirth.Value, DateTimeKind.Utc) : null,
                    CreatorCF = pratica.CreatorCF,
                    FileName = pratica.FileName,
                    ContentBytes = pratica.ContentBytes,
                    CreationDate = DateTime.SpecifyKind(pratica.CreationDate, DateTimeKind.Utc),
                    CreatorId = pratica.CreatorId,
                    Status = pratica.Status
                });
                await this.dbContext.SaveChangesAsync();
                return ConstantValues.ServicesHttpResponses.OK;
            }
            return ConstantValues.ServicesHttpResponses.ItemDoesntExist;
        }
    }
}
