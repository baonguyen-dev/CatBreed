using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CatBreed.Entities.Base;
using CatBreed.Repositories.Base.Services;
using SQLite;

namespace CatBreed.Repositories.Base
{
    public abstract class BaseRepository<T> where T : BaseEntity, new()
    {
        protected abstract string DbPath { get; }
        protected string LanguageDatabaseName;

        protected SQLiteConnection NewConnection()
        {
            return new SQLiteConnection(DbPath,
                            SQLiteOpenFlags.ReadWrite |
                            SQLiteOpenFlags.Create |
                            SQLiteOpenFlags.FullMutex |
                            SQLiteOpenFlags.SharedCache, false);
        }

        public BaseRepository()
        {
            LanguageDatabaseName = typeof(T).Name.ToUpper() + "_LANGUAGE.DB";

            var properties = typeof(T).GetRuntimeProperties();

            using (var db = NewConnection())
            {
                db.CreateTable<T>();
            }
        }

        public T Load(UInt32 id)
        {
            return Load(p => p.Id == id);
        }

        public virtual IList<T> LoadAll<U>(Expression<Func<T, bool>> expression, Expression<Func<T, U>> orderExpression, bool isDescending = false)
        {
            if (orderExpression == null)
            {
                return LoadAll(expression);
            }

            using (var db = NewConnection())
            {
                IList<T> result = null;

                if (expression == null)
                {
                    result = isDescending ? db.GetAllOrderDescendingWithChildren<T, U>(orderExpression)
                        : db.GetAllOrderWithChildren<T, U>(orderExpression);
                }
                else
                {
                    result = isDescending ? db.GetAllOrderDescendingWithChildren<T, U>(expression, orderExpression)
                        : db.GetAllOrderWithChildren<T, U>(expression, orderExpression);
                }

                return result;
            }
        }

        public virtual IList<T> LoadAll(Expression<Func<T, bool>> expression = null)
        {
            using (var db = NewConnection())
            {
                IList<T> result = null;

                if (expression != null)
                {
                    result = db.GetAllWithChildren<T>(expression);
                }
                else
                {
                    result = db.GetAllWithChildren<T>();
                }

                return result;
            }
        }

        public virtual IList<T> LoadAllWithoutChildren<U>(Expression<Func<T, bool>> expression, Expression<Func<T, U>> orderExpression, bool isDescending = false)
        {
            if (orderExpression == null)
            {
                return LoadAllWithoutChildren(expression);
            }

            using (var db = NewConnection())
            {
                IList<T> result = null;

                if (expression == null)
                {
                    result = isDescending ? db.Table<T>().OrderByDescending(orderExpression).ToList()
                        : db.Table<T>().OrderBy(orderExpression).ToList();
                }
                else
                {
                    result = isDescending ? db.Table<T>().Where(expression).OrderByDescending(orderExpression).ToList()
                        : db.Table<T>().Where(expression).OrderBy(orderExpression).ToList();
                }

                return result;
            }
        }

        public virtual int Count(Expression<Func<T, bool>> expression = null)
        {
            using (var db = NewConnection())
            {
                if (expression != null)
                {
                    return db.Table<T>().Count(expression);
                }
                else
                {
                    return db.Table<T>().Count<T>();
                }
            }
        }

        public virtual IList<T> LoadAllWithoutChildren(Expression<Func<T, bool>> expression = null)
        {
            using (var db = NewConnection())
            {
                IList<T> result = null;

                if (expression != null)
                {
                    result = db.Table<T>().Where(expression).ToList();
                }
                else
                {
                    result = db.Table<T>().ToList();
                }

                return result;
            }
        }

        public virtual T Load(Expression<Func<T, bool>> expression = null)
        {
            using (var db = NewConnection())
            {
                T result = null;

                if (expression == null)
                {
                    result = db.GetWithChildren<T>();
                }
                else
                {
                    result = db.GetWithChildren<T>(expression);
                }

                return result;
            }
        }

        public virtual IList<T> LoadAllInList(IList<uint> ids, string fieldName = "Id")
        {
            if (ids == null || !ids.Any())
                return null;

            using (var db = NewConnection())
            {
                var result = db.GetAllWithChildren<T>(ids, fieldName);

                return result;
            }
        }

        public virtual T Load<U>(Expression<Func<T, bool>> expression, Expression<Func<T, U>> orderExpression, bool isDescending = false)
        {
            if (orderExpression == null)
                return Load(expression);

            using (var db = NewConnection())
            {
                T result = null;

                if (expression == null)
                {
                    result = isDescending ? db.GetOrderDescendingWithChildren<T, U>(orderExpression) : db.GetOrderWithChildren<T, U>(orderExpression);
                }
                else
                {
                    result = isDescending ? db.GetOrderDescendingWithChildren<T, U>(expression, orderExpression) : db.GetOrderWithChildren<T, U>(expression, orderExpression);
                }

                return result;
            }
        }

        public virtual T LoadWithoutChildren<U>(Expression<Func<T, bool>> expression, Expression<Func<T, U>> orderExpression, bool isDescending = false)
        {
            if (orderExpression == null)
                return LoadWithoutChildren(expression);

            using (var db = NewConnection())
            {
                T result = null;

                if (expression == null)
                {
                    result = isDescending ? db.Table<T>().OrderByDescending(orderExpression).FirstOrDefault() : db.Table<T>().OrderBy(orderExpression).FirstOrDefault();
                }
                else
                {
                    result = isDescending ? db.Table<T>().OrderByDescending(orderExpression).FirstOrDefault(expression) : db.Table<T>().OrderBy(orderExpression).FirstOrDefault(expression);
                }

                return result;
            }
        }

        public virtual T LoadWithoutChildren(Expression<Func<T, bool>> expression = null)
        {
            using (var db = NewConnection())
            {
                T result = null;

                if (expression == null)
                {
                    result = db.Table<T>().FirstOrDefault();
                }
                else
                {
                    result = db.Table<T>().FirstOrDefault(expression);
                }

                return result;
            }
        }

        //***Insert***

        public virtual void InsertWithoutChildren(T entity)
        {
            using (var db = NewConnection())
            {

                db.Insert(entity);
            }
        }

        public virtual int Insert(T entity)
        {
            using (var db = NewConnection())
            {

                var insert = db.InsertWithChildren<T>(entity);

                return insert;
            }
        }

        public virtual int InsertAll(IList<T> entities)
        {
            using (var db = NewConnection())
            {

                var ret = db.InsertAllWithChildren(entities);

                return ret;
            }
        }

        public virtual int InsertAllWithoutChildren(IList<T> entities)
        {
            using (var db = NewConnection())
            {
                var ret = db.InsertAll(entities);

                return ret;
            }
        }

        public virtual int InsertOrReplace(T entity)
        {
            using (var db = NewConnection())
            {

                var ret = db.InsertOrReplaceWithChildren<T>(entity);

                return ret;
            }
        }

        public virtual int Update(T entity)
        {
            using (var db = NewConnection())
            {

                var ret = db.UpdateWithChildren(entity);

                return ret;
            }
        }

        public virtual int UpdateWithoutChildren(T entity)
        {
            using (var db = NewConnection())
            {

                var ret = db.Update(entity);

                return ret;
            }
        }

        public virtual int UpdateAll(IList<T> entities)
        {
            using (var db = NewConnection())
            {
                var ret = db.UpdateAllWithChildren(entities);

                return ret;
            }
        }

        public virtual int Delete(T o)
        {
            using (var db = NewConnection())
            {
                return db.Delete(o);
            }
        }

        public virtual int Delete(uint id)
        {
            using (var db = NewConnection())
            {
                return db.Delete<T>(id);
            }
        }

        public virtual void DeleteAll()
        {
            using (var db = NewConnection())
            {
                db.DeleteAll<T>();
            }
        }

        public virtual int DeleteAll(IList<T> entities)
        {
            using (var db = NewConnection())
            {
                return db.DeleteAll<T>(entities);
            }
        }

        public virtual void Delete(IList<T> ids)
        {
            using (var db = NewConnection())
            {
                db.DeleteAll(ids);
            }
        }
    }
}

