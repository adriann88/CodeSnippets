
'''csharp
public static IQueryable<TEntity> WhereIsSnapshot<TEntity>(IQueryable<TEntity> query, Expression<Func<ISnapshot, bool>> predicate) 
            where TEntity : class, IDeletable
        {            
            query = ((IQueryable<ISnapshot>)query)
                .Where(predicate)
                .Cast<TEntity>();
            return query;
        }
public static IQueryable<T> ActiveSet<T>(this System.Data.Entity.DbContext context) 
            where T : class, IDeletable 
        {
            string currentType = typeof(T).FullName;

            if (typeof(ISnapshot).IsAssignableFrom(typeof(T)))
            {
                var resultIsDel = context.Set<T>().Where(f => f.IsDeleted == false);
                return WhereIsSnapshot<T>(resultIsDel, x => x.IsSnapshot == false);                
            }
            return context.Set<T>().Where(f => f.IsDeleted == false);
        }
'''
