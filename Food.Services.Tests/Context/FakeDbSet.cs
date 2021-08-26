using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Food.Services.Tests.Context
{
    class FakeDbSet<TEntity, TI> : DbSet<TEntity>, IQueryable, IEnumerable<TEntity>, IAsyncEnumerable<TEntity>
        where TEntity : EntityBaseDeletable<TI>
    {
        private readonly ObservableCollection<TEntity> _data;
        private readonly IQueryable _query;

        public FakeDbSet()
        {

            _data = new ObservableCollection<TEntity>();
            _query = _data.AsQueryable(); 
        }

        public override EntityEntry<TEntity> Add(TEntity item)
        {
            var type = typeof(TEntity);
            var p = type.GetProperty("Id");
            p.SetValue(item, (short)_data.Count);
            _data.Add(item);
            return null;
        }

        public override void AddRange(IEnumerable<TEntity> entities)
        {
            foreach (var item in entities)
            {
                var type = typeof(TEntity);
                var p = type.GetProperty("Id");
                p.SetValue(item, (short)_data.Count);
                _data.Add(item);
            }
        }

        public override EntityEntry<TEntity> Remove(TEntity item)
        {
            (item as EntityBase<TI>).IsDeleted = true;
            return null;
        }

        public override void RemoveRange(IEnumerable<TEntity> entities) 
        {
            if (entities.First() is EntityBase<TI>)
            {
                foreach (var entity in entities)
                {
                    (entity as EntityBase<TI>).IsDeleted = true;
                }
            }
            else
            {
                var ids = entities.Select(e => e.Id).ToList();
                foreach (var id in ids)
                {
                    var toDel = _data.First(e => e.Id.Equals(id));
                    _data.Remove(toDel);
                }
            }
        }

        public override EntityEntry<TEntity> Attach(TEntity item)
        {
            _data.Add(item);
            return null;
        }

        public override LocalView<TEntity> Local
        {
            get { throw new NotImplementedException(); }
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new TestQueryProvider<TEntity>(_query.Provider); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public IAsyncEnumerator<TEntity> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    internal class TestDbAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestDbAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestDbAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        IQueryProvider IQueryable.Provider
        {
            get { return new TestQueryProvider<T>(this); }
        }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    internal class TestDbAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestDbAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public T Current
        {
            get { return _inner.Current; }
        }
    }
}