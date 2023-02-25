using System;
using CatBreed.Entities.Base;
using SQLite;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace CatBreed.Repositories.Base.Services
{
    public static class SQLiteConnectionExt
    {
        public static IList<T> GetAllWithChildren<T>(this SQLiteConnection conn, Expression<Func<T, bool>> expression) where T : BaseEntity, new()
        {
            var list = conn?.Table<T>()?.Where(expression).ToList();

            return Deserialize<T>(list);
        }

        public static IList<T> GetAllWithChildren<T>(this SQLiteConnection conn, IList<uint> ids, string fieldName) where T : BaseEntity, new()
        {
            var rawList = "(";

            foreach (var id in ids)
            {
                rawList += $"{id.ToString()},";
            }

            rawList = rawList.Remove(rawList.Length - 1);

            rawList += ")";

            var list = conn.Query<T>($"SELECT * FROM {(typeof(T)).Name} WHERE {fieldName} IN {rawList};");

            return Deserialize<T>(list);
        }

        public static IList<T> GetAllOrderWithChildren<T, U>(this SQLiteConnection conn, Expression<Func<T, bool>> expression, Expression<Func<T, U>> orderExpression) where T : BaseEntity, new()
        {
            var list = conn?.Table<T>()?.Where(expression).OrderBy(orderExpression).ToList();

            return Deserialize<T>(list);
        }

        public static IList<T> GetAllOrderWithChildren<T, U>(this SQLiteConnection conn, Expression<Func<T, U>> orderExpression) where T : BaseEntity, new()
        {
            var list = conn?.Table<T>()?.OrderBy(orderExpression).ToList();

            return Deserialize<T>(list);
        }

        public static IList<T> GetAllOrderDescendingWithChildren<T, U>(this SQLiteConnection conn, Expression<Func<T, bool>> expression, Expression<Func<T, U>> orderExpression) where T : BaseEntity, new()
        {
            var list = conn?.Table<T>()?.Where(expression).OrderByDescending(orderExpression).ToList();

            return Deserialize<T>(list);
        }

        public static IList<T> GetAllOrderDescendingWithChildren<T, U>(this SQLiteConnection conn, Expression<Func<T, U>> orderExpression) where T : BaseEntity, new()
        {
            var list = conn?.Table<T>().OrderByDescending(orderExpression).ToList();

            return Deserialize<T>(list);
        }

        public static IList<T> GetAllWithChildren<T>(this SQLiteConnection conn) where T : BaseEntity, new()
        {
            var list = conn?.Table<T>()?.ToList();

            return Deserialize<T>(list);
        }

        public static T GetWithChildren<T>(this SQLiteConnection conn, Expression<Func<T, bool>> expression) where T : BaseEntity, new()
        {
            return Deserialize<T>(conn.Table<T>().FirstOrDefault(expression));
        }

        public static T GetWithChildren<T>(this SQLiteConnection conn) where T : BaseEntity, new()
        {
            return Deserialize<T>(conn.Table<T>()?.FirstOrDefault());
        }

        public static T GetWithChildren<T>(this SQLiteConnection conn, uint id) where T : BaseEntity, new()
        {
            return Deserialize<T>(conn.Table<T>().FirstOrDefault(p => p.Id == id));
        }

        public static T GetOrderWithChildren<T, U>(this SQLiteConnection conn, Expression<Func<T, bool>> expression, Expression<Func<T, U>> orderExpression) where T : BaseEntity, new()
        {
            return Deserialize<T>(conn.Table<T>().OrderBy(orderExpression).FirstOrDefault(expression));
        }

        public static T GetOrderDescendingWithChildren<T, U>(this SQLiteConnection conn, Expression<Func<T, bool>> expression, Expression<Func<T, U>> orderExpression) where T : BaseEntity, new()
        {
            return Deserialize<T>(conn.Table<T>().OrderByDescending(orderExpression).FirstOrDefault(expression));
        }

        public static T GetOrderWithChildren<T, U>(this SQLiteConnection conn, Expression<Func<T, U>> orderExpression) where T : BaseEntity, new()
        {
            return Deserialize<T>(conn.Table<T>().OrderBy(orderExpression).FirstOrDefault());
        }

        public static T GetOrderDescendingWithChildren<T, U>(this SQLiteConnection conn, Expression<Func<T, U>> orderExpression) where T : BaseEntity, new()
        {
            return Deserialize<T>(conn.Table<T>().OrderByDescending(orderExpression).FirstOrDefault());
        }

        public static int DeleteAll<T>(this SQLiteConnection conn, IList<T> items) where T : BaseEntity, new()
        {
            var count = 0;

            foreach (var item in items)
            {
                if (item != null)
                    count += conn.Delete(item);
            }

            return count;
        }

        public static int InsertWithChildren<T>(this SQLiteConnection conn, T obj) where T : BaseEntity, new()
        {
            if (obj == null)
                return conn.Insert(obj);

            if (!obj.GetType().IsSerializable())
                return conn.Insert(obj);

            obj.Blob = obj.Serialize();

            return conn.Insert(obj);
        }


        public static int UpdateWithChildren<T>(this SQLiteConnection conn, T obj) where T : BaseEntity, new()
        {
            if (obj == null)
                return conn.Update(obj);

            if (!obj.GetType().IsSerializable())
                return conn.Update(obj);

            obj.Blob = obj.Serialize();

            return conn.Update(obj);
        }

        public static int InsertAllWithChildren<T>(this SQLiteConnection conn, IList<T> objects, bool runInTransaction = true) where T : BaseEntity, new()
        {
            if (objects == null)
                conn.InsertAll(objects, runInTransaction);

            foreach (var obj in objects)
            {
                if (!obj.GetType().IsSerializable())
                    continue;

                obj.Blob = obj.Serialize();
            }

            return conn.InsertAll(objects, runInTransaction);
        }

        public static int UpdateAllWithChildren<T>(this SQLiteConnection conn, IList<T> objects, bool runInTransaction = true) where T : BaseEntity, new()
        {
            if (objects == null)
                conn.UpdateAll(objects, runInTransaction);

            foreach (var obj in objects)
            {
                if (!obj.GetType().IsSerializable())
                    continue;

                obj.Blob = obj.Serialize();
            }

            return conn.UpdateAll(objects, runInTransaction);
        }

        public static int InsertOrReplaceWithChildren<T>(this SQLiteConnection conn, T obj) where T : BaseEntity, new()
        {
            return obj.Id > 0 ? conn.UpdateWithChildren<T>(obj) : conn.InsertWithChildren(obj);
        }


        public static T Deserialize<T>(T item) where T : BaseEntity, new()
        {
            if (!typeof(T).IsSerializable())
                return item;

            if (item == null || item.Blob == null || item.Blob.Length <= 0)
                return item;

            var dbId = item.Id;

            var newResult = item.Blob.Deserialize<T>();

            newResult.Id = dbId;

            return newResult;
        }

        static IList<T> Deserialize<T>(IList<T> list) where T : BaseEntity, new()
        {
            if (!typeof(T).IsSerializable())
                return list;

            if (list == null || !list.Any())
                return list;

            var newList = new List<T>();

            for (var i = 0; i < list.Count; i++)
            {
                var result = list[i];

                if (result.Blob == null || result.Blob.Length <= 0)
                {
                    newList.Add(result);
                    continue;
                }

                var dbId = result.Id;

                var newResult = result.Blob.Deserialize<T>();

                newResult.Id = dbId;

                newList.Add(newResult);
            }

            return newList;
        }
    }

    public static class SerializeUtil
    {
        public static bool IsSerializable(this Type type)
        {
            var serialAttrs = Attribute.GetCustomAttribute(type, typeof(SerializableAttribute));

            return serialAttrs != null;
        }

        public static T Deserialize<T>(this byte[] arrBytes) where T : BaseEntity
        {
            try
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    BinaryFormatter binForm = new BinaryFormatter();
                    memStream.Write(arrBytes, 0, arrBytes.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    var obj = (T)binForm.Deserialize(memStream);

                    if (obj != null)
                        obj.Blob = null;

                    return obj;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("++++++++++" + ex);

                return null;
            }
        }

        public static byte[] Serialize(this object obj)
        {
            if (obj == null)
                return null;


            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}

