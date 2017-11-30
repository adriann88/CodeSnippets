///QueryableExtensions.cs
using EntityFramework.Extensions;
using System;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Project
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Gets only the records that are neither deleted nor snapshots.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a query that filters entities based of the implementation of the ISnapshot interface
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicate">The filtering condition.</param>
        /// <returns></returns>
        public static IQueryable<TEntity> WhereIsSnapshot<TEntity>(IQueryable<TEntity> query, Expression<Func<ISnapshot, bool>> predicate) 
            where TEntity : class, IDeletable
        {            
            query = ((IQueryable<ISnapshot>)query)
                .Where(predicate)
                .Cast<TEntity>();
            return query;
        }        
    }
}

///Interfaces used in the code above
//IDeletable.cs
    /// <summary>
    /// Logical deletable entity
    /// </summary>
    public interface IDeletable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This will mark the entity as deleted, instead of physically removing it.
        /// </remarks>
        bool IsDeleted { get; set; }
    }

//ISnapshot.cs
    /// <summary>
    /// Logical snapshot entity
    /// </summary>
    public interface ISnapshot
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is a snapshot.
        /// </summary>
        /// <value>
        /// If the value is true, this instance should not be present in Context
        /// </value>        
        bool IsSnapshot { get; set; }
    }

    /// <summary>
    /// Gets only SnapshotEntities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IQueryable<T> IDataContext.SnapshotEntities<T>()
    {
        return Set<T>().Where(f => f.IsSnapshot == true);
    }

///Extending the database context
    /// <summary>
    /// Gets only logically deleted entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IQueryable<T> IDataContext.RecycleBin<T>()
    {
        return Set<T>().Where(f => f.IsDeleted == true);
    }

    /// <summary>
    /// Gets only the active entities. Deleted and Snapshot entities are excluded.
    /// </summary>
    IQueryable<ComplianceMatrix> ICompanyContext.ComplianceMatrices
    {
        get { return this.ActiveSet<ComplianceMatrix>(); }
    }

//Example 

//Gets all open compliance matrices
var cmxEntities = ctx.ComplianceMatrices.Where(f => f.EndDate < DateTime.Now);
//Gets all opne compliance matrices snapshots
var cmxSnapshotEntities = ctx.SnapshotEntities<ComplianceMatrix>().Where(f => f.EndDate < DateTime.Now);

