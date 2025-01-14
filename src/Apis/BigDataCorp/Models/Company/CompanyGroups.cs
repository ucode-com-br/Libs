using System;
using System.Collections;
using System.Collections.Generic;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    [DatasetInfo("company_group_documentroot", "CompanyGroups")]
    public class CompanyGroups : IList<CompanyGroup>
    {
        private readonly List<CompanyGroup> _companyGroups = new List<CompanyGroup>();

        public CompanyGroups()
        {
        }

        public CompanyGroup this[int index] { get => _companyGroups[index]; set => _companyGroups[index] = value; }

        public int Count => _companyGroups.Count;

        public bool IsReadOnly => false;

        public void Add(CompanyGroup item) => _companyGroups.Add(item);

        public void Clear() => _companyGroups.Clear();

        public bool Contains(CompanyGroup item) => _companyGroups.Contains(item);

        public void CopyTo(CompanyGroup[] array, int arrayIndex) => _companyGroups.CopyTo(array, arrayIndex);

        public IEnumerator<CompanyGroup> GetEnumerator()
        {
            return _companyGroups.GetEnumerator();
        }

        public int IndexOf(CompanyGroup item) => _companyGroups.IndexOf(item);

        public void Insert(int index, CompanyGroup item) => _companyGroups.Insert(index, item);

        public bool Remove(CompanyGroup item) => _companyGroups.Remove(item);

        public void RemoveAt(int index) => _companyGroups.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
