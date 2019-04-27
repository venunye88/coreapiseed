using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CodeApiSeed.Models;
using CoreApiSeed.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CodeApiSeed.Services
{

    public interface IBaseService<TDto>:IModelService<TDto> where TDto : class { }

    public class BaseService<T, TModel> : IBaseService<T> where T : class where TModel : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TModel> _dbSet;
        protected readonly IMapper _mapper;

        public BaseService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _dbSet = _context.Set<TModel>();
        }

        public virtual async Task<T> Find(long id)
        {
            var record = _dbSet.Find(id);
            return await Task.FromResult(record != null ? _mapper.Map<T>(record) : null);
        }

        public virtual async Task<List<T>> FetchAll()
        {
            return await Task.FromResult(_mapper.Map<List<T>>(_dbSet.ToList()));
        }



        public virtual async Task<long> Save(T record)
        {
            var model = _mapper.Map<TModel>(record);
            await _dbSet.AddAsync(model);
            _context.SaveChanges();

            var recordId = (model as HasId)?.Id ?? 0;
            return recordId;
        }

        public virtual async Task<long> Update(T record)
        {
            var rec = JsonConvert.DeserializeObject<HasId>(JsonConvert.SerializeObject(record));
            var data = await _dbSet.FindAsync(rec.Id);
            _mapper.Map(record, data);
            _dbSet.Update(data);
            _context.SaveChanges();
            return rec.Id;
        }

        public async Task<bool> Delete(long id)
        {
            var record = await _dbSet.FindAsync(id);
            _dbSet.Remove(record);
            _context.SaveChanges();
            return true;
        }

      
    }
}
