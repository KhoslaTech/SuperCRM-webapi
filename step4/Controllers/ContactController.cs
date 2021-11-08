using ASPSecurityKit;
using ASPSecurityKit.Net;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SuperCRM.DataModels;
using SuperCRM.ModelBinding;
using SuperCRM.Models;

namespace SuperCRM.Controllers
{
    [Route("contacts")]
    public class ContactController : SiteControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;

        public ContactController(IUserService<Guid, Guid, DbUser> userService, INetSecuritySettings securitySettings,
	        ISecurityUtility securityUtility, IConfig config, AppDbContext dbContext, IMapper mapper) : base(userService, securitySettings, securityUtility,
	        config)
        {
	        this.dbContext = dbContext;
	        this.mapper = mapper;
        }

        [HttpGet]
        [PossessesPermissionCode]
        public async Task<BaseListResponse<Contact>> List([FromQuery] GetContacts model)
        {
            Expression<Func<DbContact, bool>> predicate = c => c.OwnerId == this.UserService.CurrentUser.OwnerUserId;

            var result = new
            {
	            Total = this.dbContext.Contacts.Count(predicate),
	            ThisPage = await this.dbContext.Contacts.Where(predicate)
		            .OrderBy(p => p.Name).Skip(model.StartIndex).Take(model.PageSize)
		            .AsQueryable()
		            .ProjectTo<Contact>(mapper.ConfigurationProvider)
		            .ToListAsync()
            };

            return Ok(result.ThisPage, result.Total);
        }

        [HttpPost]
        [PossessesPermissionCode]
        public async Task<BaseResponse> Create(Contact model)
        {
            if (ModelState.IsValid)
            {
                var entity = mapper.Map<DbContact>(model);
                entity.OwnerId = this.UserService.CurrentUser.OwnerUserId;
                entity.CreatedById = this.UserService.CurrentUserId;
                this.dbContext.Contacts.Add(entity);
                await this.dbContext.SaveChangesAsync();
                return Ok(mapper.Map<Contact>(entity));
            }

            return Error();
        }

        [HttpPut]
        [Route("{Id}")]
        public async Task<BaseResponse> Edit([FromBodyAndRoute] Contact model)
        {
            if (ModelState.IsValid)
            {
                var entity = await this.dbContext.Contacts.FindAsync(model.Id);
                if (entity == null)
                    return Error(OpResult.DoNotExist, "Contact not found.");

                mapper.Map(model, entity);
                await this.dbContext.SaveChangesAsync();
                return Ok(mapper.Map<Contact>(entity));
            }

            return Error();
        }

        [HttpDelete]
        [Route("{contactId}")]
        public async Task<BaseResponse> Delete(Guid contactId)
        {
            var entity = await this.dbContext.Contacts.Include(x => x.Interactions)
            .SingleOrDefaultAsync(x => x.Id == contactId);
            if (entity == null)
                return Error(OpResult.DoNotExist, "Contact not found.");

            this.dbContext.Remove(entity);
            await this.dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}