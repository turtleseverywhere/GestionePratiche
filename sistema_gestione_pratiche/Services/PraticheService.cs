using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using sistema_gestione_pratiche.Common;
using sistema_gestione_pratiche.DB.Contexts;
using sistema_gestione_pratiche.DB.Entities;
using sistema_gestione_pratiche.DB.Entities.AuthEntities;
using sistema_gestione_pratiche.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static sistema_gestione_pratiche.Common.ConstantValues.Entities;

namespace sistema_gestione_pratiche.Services
{
    public class PraticheService
    {
        private DBContext dbContext;
        UserManager<User> userManager;
        private SignalRService signalRService;
        private NotificationsService notificationsService;
        public PraticheService(DBContext dBContext
            , UserManager<User> userManager
            , SignalRService signalRService
            , NotificationsService notificationsService)
        {
            this.dbContext = dBContext;
            this.userManager = userManager;
            this.signalRService = signalRService;
            this.notificationsService = notificationsService;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> GetPratiche(ClaimsPrincipal user)
        {
            string? userId = this.userManager.GetUserId(user);
            if (!string.IsNullOrEmpty(userId))
            {
                var pratiche = this.dbContext.Pratiche
                     .Where(p => p.CreatorId == Guid.Parse(userId))
                     .OrderByDescending(n => n.CreationDate)
                     .ToList();

                return (pratiche, HttpStatusCode.OK);
            }
            return ConstantValues.ServicesHttpResponses.BadRequest;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> InsertPratica(ClaimsPrincipal user, PraticaModel praticaModel)
        {
            string? userId = this.userManager.GetUserId(user);
            if (!string.IsNullOrEmpty(userId))
            {
                var pratica = this.dbContext.Pratiche.Add(new DB.Entities.Pratica()
                {
                    Id = Guid.NewGuid(),
                    CreatorName = praticaModel.CreatorName,
                    CreatorLastName = praticaModel.CreatorLastName,
                    CreatorDateOfBirth = praticaModel.CreatorDateOfBirth,
                    CreatorCF = praticaModel.CreatorCF,
                    FileName = praticaModel.FileName,
                    ContentBytes = praticaModel.ContentBytes,
                    CreationDate = DateTime.UtcNow,
                    CreatorId = Guid.Parse(userId),
                    Status = ConstantValues.Entities.Pratiche.Status.Created
                }).Entity;
                await this.dbContext.SaveChangesAsync();
                await this.notificationsService.InsertNotification(pratica.CreatorId, "Pratica created " + pratica.Id + " " + pratica.FileName);
                await this.signalRService.SendMessageToUser(userId, "Pratica created " + pratica.Id + " " + pratica.FileName);
                return (pratica, HttpStatusCode.OK);
            }
            return ConstantValues.ServicesHttpResponses.BadRequest;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> UpdatePratica(PraticaModel praticaModel)
        {
            var pratica = this.dbContext.Pratiche
                     .Where(p => p.Id == praticaModel.Id)
                     .FirstOrDefault();
            if (pratica != null)
            {
                if (pratica.Status == ConstantValues.Entities.Pratiche.Status.Created)
                {
                    if (!string.IsNullOrEmpty(praticaModel.CreatorName))
                        pratica.CreatorName = praticaModel.CreatorName;
                    if (!string.IsNullOrEmpty(praticaModel.CreatorLastName))
                        pratica.CreatorLastName = praticaModel.CreatorLastName;
                    if (!string.IsNullOrEmpty(praticaModel.CreatorCF))
                        pratica.CreatorCF = praticaModel.CreatorCF;
                    if (praticaModel.CreatorDateOfBirth != null)
                        pratica.CreatorDateOfBirth = praticaModel.CreatorDateOfBirth;
                    if (praticaModel.FileName != null)
                        pratica.FileName = praticaModel.FileName;
                    if (praticaModel.ContentBytes != null)
                        pratica.ContentBytes = praticaModel.ContentBytes;
                    await this.dbContext.SaveChangesAsync();
                    return ConstantValues.ServicesHttpResponses.OK;
                }
            }
            return ConstantValues.ServicesHttpResponses.BadRequest;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> ChangePraticaStatus(PraticaModel praticaModel)
        {
            var pratica = this.dbContext.Pratiche
                     .Where(p => p.Id == praticaModel.Id)
                     .FirstOrDefault();
            if (pratica != null)
            {
                /*<da mettere nel servizio esterno>*/
                pratica.Status = praticaModel.Status;
                await this.dbContext.SaveChangesAsync();
                /*</da mettere nel servizio esterno>*/
                await this.notificationsService.InsertNotification(pratica.CreatorId,
                    "Status updated "
                    + pratica.Id + " "
                    + pratica.FileName + " "
                    + pratica.Status);
                await this.signalRService.SendMessageToUser(pratica.CreatorId.ToString(),
                    "Status updated "
                    + pratica.Id + " "
                    + pratica.FileName + " "
                    + pratica.Status);
                return ConstantValues.ServicesHttpResponses.OK;
            }
            return ConstantValues.ServicesHttpResponses.ItemDoesntExist;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> DeletePratica(PraticaModel praticaModel)
        {
            if (praticaModel.Id.HasValue)
            {
                var praticaToDelete = this.dbContext.Pratiche
                         .Where(p => p.Id == praticaModel.Id).FirstOrDefault();
                if (praticaToDelete != null)
                {
                    this.dbContext.Remove(praticaToDelete);
                    await this.dbContext.SaveChangesAsync();
                    await this.notificationsService.InsertNotification(praticaToDelete.CreatorId, "Pratica deleted " + praticaToDelete.Id);
                    await this.signalRService.SendMessageToUser(praticaToDelete.CreatorId.ToString(), "Pratica deleted " + praticaToDelete.Id);
                    return ConstantValues.ServicesHttpResponses.OK;
                }
                return ConstantValues.ServicesHttpResponses.ItemDoesntExist;
            }
            return ConstantValues.ServicesHttpResponses.BadRequest;
        }

        public async Task<(object Result, HttpStatusCode StatusCode)> DownloadPratica(PraticaModel praticaModel)
        {
            if (praticaModel.Id.HasValue)
            {
                var pratica = this.dbContext.Pratiche
                         .Where(p => p.Id == praticaModel.Id).FirstOrDefault();
                if (pratica?.ContentBytes != null)
                    return (pratica, HttpStatusCode.OK);
                return ConstantValues.ServicesHttpResponses.ItemDoesntExist;
            }
            return ConstantValues.ServicesHttpResponses.BadRequest;
        }
    }
}