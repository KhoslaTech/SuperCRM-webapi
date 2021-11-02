using ASPSecurityKit;
using ASPSecurityKit.Net;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperCRM.DataModels;
using SuperCRM.ModelBinding;
using SuperCRM.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SuperCRM.Controllers
{
    [Route("interactions")]
    [Route("contacts/{ContactId}/interactions")]
    public class InteractionController : SiteControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;

        public InteractionController(IUserService<Guid, Guid, DbUser> userService,
	        INetSecuritySettings securitySettings, ISecurityUtility securityUtility, IConfig config, AppDbContext dbContext, IMapper mapper) : base(userService,
	        securitySettings, securityUtility, config)
        {
	        this.dbContext = dbContext;
	        this.mapper = mapper;
        }

        [HttpGet]
        [PossessesPermissionCode, AuthPermission]
        public async Task<BaseListResponse<Interaction>> GetInteractions([FromQuery] GetInteractions model)
        {
            Func<DbInteraction, bool> predicate;
            if (model.ContactId.HasValue)
                predicate = i => i.ContactId == model.ContactId.Value;
            else
                predicate = i => i.Contact.OwnerId == this.UserService.CurrentUser.OwnerUserId;

            var result = new
            {
	            Total = this.dbContext.Interactions.Count(predicate),
	            ThisPage = await this.dbContext.Interactions.Where(predicate)
		            .OrderByDescending(p => p.InteractionDate).Skip(model.StartIndex).Take(model.PageSize)
		            .AsQueryable()
		            .ProjectTo<Interaction>(mapper.ConfigurationProvider)
		            .ToListAsync()
            };

            return Ok(result.ThisPage, result.Total);
        }

        [HttpPost]
        public async Task<BaseResponse> Add(Interaction model)
        {
            if (ModelState.IsValid)
            {
                var entity = mapper.Map<DbInteraction>(model);
                entity.CreatedById = this.UserService.CurrentUserId;
                this.dbContext.Interactions.Add(entity);
                await this.dbContext.SaveChangesAsync();
                return Ok(mapper.Map<Interaction>(entity));
            }

            return Error();
        }

        [HttpPut]
        [Route("{Id}")]
        public async Task<BaseResponse> Edit([FromBodyAndRoute] Interaction model)
        {
            if (ModelState.IsValid)
            {
                var entity = await this.dbContext.Interactions.FindAsync(model.Id);
                if (entity == null)
                    return Error(OpResult.DoNotExist, "Interaction not found.");

                mapper.Map(model, entity);
                await this.dbContext.SaveChangesAsync();
                return Ok(mapper.Map<Interaction>(entity));
            }

            return Error();
        }

        [HttpDelete]
        [Route("{interactionId}")]
        public async Task<BaseResponse> Delete(Guid interactionId)
        {
            var entity = await this.dbContext.Interactions.FindAsync(interactionId);
            if (entity == null)
                return Error(OpResult.DoNotExist, "Interaction not found.");

            this.dbContext.Remove(entity);
            await this.dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}