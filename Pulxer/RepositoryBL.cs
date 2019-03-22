using Common.Data;
using Common.Interfaces;
using System.Xml.Linq;

namespace Pulxer
{
    /// <summary>
    /// Используется для хранения в постоянной памяти различных объектов по ключу.
    /// </summary>
    public class RepositoryBL : IRepositoryBL
    {
        private readonly IRepositoryDA _reposDA;

        public RepositoryBL(IRepositoryDA da)
        {
            _reposDA = da;
        }

        /// <summary>
        /// Сохранить в репозитории xml-сериализуемый объект
        /// </summary>
        /// <typeparam name="T">Тип сериализуемого объекта</typeparam>
        /// <param name="obj">Сериализуемый объект</param>
        /// <param name="key">Ключ. Если с таким ключом объект уже существует, он будет перезаписан новым объектом. Идентификатор записи при этом не измеится.</param>
        /// <returns>Идентификатор записи (позволяет однозначно идентифицировать запись)</returns>
        public int PutXmlObject<T>(T obj, string key = "") where T : IXmlSerializable, new()
        {
            ReposObject ro = _reposDA.Select(0, key);
            string data = obj.Serialize().ToString();
            if (ro == null)
            {
                ro = _reposDA.Create(key, data);
            }
            else
            {
                ro.Data = data;
                _reposDA.Update(ro);
            }

            return ro.ReposID;
        }

        /// <summary>
        /// Сохранить в репозитории string-сериализуемый объект
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="obj">Сериализуемый объект</param>
        /// <param name="key">Ключ. Если с таким ключом объект уже существует, он будет перезаписан новым объектом. Идентификатор записи при этом не измеится.</param>
        /// <returns>Идентификатор записи (позволяет однозначно идентифицировать запись)</returns>
        public int PutStrObject<T>(T obj, string key = "") where T : IStrSerializable, new()
        {
            ReposObject ro = _reposDA.Select(0, key);
            string data = obj.Serialize();
            if (ro == null)
            {
                ro = _reposDA.Create(key, data);
            }
            else
            {
                ro.Data = data;
                _reposDA.Update(ro);
            }

            return ro.ReposID;
        }

        /// <summary>
        /// Извлечь xml-сериализуемый объект из хранилища и удалить его в хранилище
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="id">Идентификатор записи (однозначно идентифицирует запись)</param>
        /// <returns>Извлеченный объект или объект по умолчанию нужного типа</returns>
        public T GetXmlObject<T>(int id) where T : IXmlSerializable, new()
        {
            return GetXmlObject<T>(id, "");
        }

        /// <summary>
        /// Извлечь xml-сериализуемый объект из хранилища и удалить его в хранилище
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ. С одним ключом хранится не более одного объекта.</param>
        /// <returns>Извлеченный объект или объект по умолчанию нужного типа</returns>
        public T GetXmlObject<T>(string key) where T : IXmlSerializable, new()
        {
            return GetXmlObject<T>(0, key);
        }

        private T GetXmlObject<T>(int id, string key) where T : IXmlSerializable, new()
        {
            var ro = _reposDA.Select(id, key);
            if (ro == null) return default(T);

            _reposDA.Delete(ro.ReposID);

            var xDoc = XDocument.Parse(ro.Data);
            var t = new T();
            t.Initialize(xDoc);

            return t;
        }

        /// <summary>
        /// Извлечь string-сериализуемый объект из хранилища и удалить его в хранилище
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="id">Идентификатор записи (однозначно идертифицирует запись)</param>
        /// <returns>Извлеченный объект или объект по умолчанию нужного типа</returns>
        public T GetStrObject<T>(int id) where T : IStrSerializable, new()
        {
            return GetStrObject<T>(id, "");
        }

        /// <summary>
        /// Извлечь string-сериализуемый объект из хранилища и удалить его в хранилище
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ. С одним ключом хранится не более одного объекта.</param>
        /// <returns>Извлеченный объект или объект по умолчанию нужного типа</returns>
        public T GetStrObject<T>(string key) where T : IStrSerializable, new()
        {
            return GetStrObject<T>(0, key);
        }

        private T GetStrObject<T>(int id, string key) where T : IStrSerializable, new()
        {
            var ro = _reposDA.Select(id, key);
            if (ro == null) return default(T);

            _reposDA.Delete(ro.ReposID);
            var t = new T();
            t.Initialize(ro.Data);

            return t;
        }

        /// <summary>
        /// Получить числовое значение по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Значение или null</returns>
        public int? GetIntParam(string key)
        {
            var ro = _reposDA.Select(0, key);
            if (ro == null) return null;

            int res = 0;
            if (!int.TryParse(ro.Data, out res)) return null;

            return res;
        }

        /// <summary>
        /// Сохранить числовое значение по ключу.
        /// При сохранении другого значения с тем же ключом, старое значение будет заменено новым.
        /// </summary>
        /// <param name="key">Ключ. Обеспечивает уникальность.</param>
        /// <param name="n">Значение</param>
        public void SetIntParam(string key, int n)
        {
            var ro = _reposDA.Select(0, key);
            if (ro == null)
            {
                _reposDA.Create(key, n.ToString());
            }
            else
            {
                ro.Data = n.ToString();
                _reposDA.Update(ro);
            }
        }

        public string GetStringParam(string key)
        {
            var ro = _reposDA.Select(0, key);
            if (ro == null) return "";

            return ro.Data;
        }

        public void SetStringParam(string key, string str)
        {
            if (str == null) str = "";

            var ro = _reposDA.Select(0, key);
            if (ro == null)
            {
                _reposDA.Create(key, str);
            }
            else
            {
                ro.Data = str;
                _reposDA.Update(ro);
            }
        }
    }
}
