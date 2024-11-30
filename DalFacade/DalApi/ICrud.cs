using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DalApi;

/// <summary>
/// Defines the contract for CRUD (Create, Read, Update, Delete) operations.
/// This generic interface provides methods for working with any class type (T).
/// It includes methods to create, read, update, and delete items, 
/// as well as additional methods for filtering and deleting all items.
/// </summary>
public interface ICrud<T> where T : class
{
    void Create(T item); 
    T? Read(int id);
    IEnumerable<T> ReadAll(Func<T, bool>? filter = null); // stage 2
    void Update(T item);
    void Delete(int id); 
    void DeleteAll();
    T? Read(Func<T, bool> filter);// stage 2
}
