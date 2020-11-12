# ReHackt.Queryable.Extensions
Some useful System.Linq.IQueryable extensions such as filtering, ordering, paging...

## Install

Get it on <a href="https://www.nuget.org/packages/ReHackt.Queryable.Extensions"><img src="https://www.nuget.org/Content/gallery/img/default-package-icon.svg" height=18 style="height:18px;" /> NuGet</a>

### Package Manager Console

```
PM> Install-Package ReHackt.Queryable.Extensions
```

### .NET CLI Console

```
> dotnet add package ReHackt.Queryable.Extensions
```

## QueryableFilter

`QueryableFilter<T>` allows to dynamically filter an `IQueryable<T>` with a query string. For example, this can be useful for an API whose clients can filter a collection of entities on any of its properties, or create complex logical queries.

For example

``` csharp
string query = @"BusinessName eq ""MyCompany"" and (""john.doe"" in Email or (FirstName eq ""John"" and LastName eq ""Doe"")) and (Amount lt 1000 or IsEnabled eq false)";

if(QueryableFilter<User>.TryParse(query, out QueryableFilter<User> filter) {
    IQueryable<User> users = _userManager.Users.Filter(filter);
}
else { /* Handle invalid query */ };
```

Is equivalent to

``` csharp
IQueryable<User> users = _userManager.Users
                            .Where(u => u.BusinessName == "MyCompany"
                                && (u.Email.Contains("john.doe") || (u.FirstName == "John" && u.LastName == "Doe"))
                                && (u.Amount < 1000 || u.IsEnabled == false);
```

### Supported in query

* Boolean operators: **and**, **or**
* Comparison operators: **eq**, **gt**, **gte**, **lt**, **lte**, **in**
* Value types: **bool**, **DateTime**, **double**, **enum**, **int**, **string**
* **Parentheses**
* **Property names**

### Not yet supported (planned)

* Boolean operators: **not**
* Value types: **class**, **struct**

## IQueryable extensions

### Filtering

#### Filter

Filter allows to apply a `QueryableFilter<T>` to the input sequence using LINQ method syntax.

``` csharp
source.Filter(filter) // filter is a QueryableFilter<T>
```

Is syntactic sugar for

``` csharp
filter.Apply(source)
```

This method also allows you to directly filter the input sequence with a query string (implicitly creating a `QueryableFilter<T>`). Be careful, this can throw an argument exception if the query string is not valid.

``` csharp
source.Filter(filterQuery) // filterQuery is a string
```

Is syntactic sugar for

``` csharp
QueryableFilter<T>.TryParse(filterQuery, out QueryableFilter<T> filter) ?
    source.Filter(filter) :
    throw new ArgumentException("Invalid filter query", nameof(filterQuery))
```

#### WhereIf

``` csharp
source.WhereIf(condition, predicate)
```

Is syntactic sugar for

``` csharp
condition ? source.Where(predicate) : source
```

This allows you to keep the LINQ method syntax to apply filters according to a condition that does not depend on the element being tested.

For example

``` csharp
return source
           .Join(...)
           .Where(...)
           .WhereIf(condition1, predicate1)
           .WhereIf(condition2, predicate2)
           .OrderBy(...)
           .Select(...);
```

Is equivalent to

``` csharp
source = source
             .Join(...)
             .Where(...);

if(condition1) {
    source = source.Where(predicate1);
} 

if(condition2) {
    source = source.Where(predicate2);
}

return source
           .OrderBy(...)
           .Select(...);
```

### Ordering

#### OrderBy, OrderByDescending, ThenBy, ThenByDescending

These methods allow you to order dynamically an input sequence by a property whose name is taken as a string. OrderBy and OrderByDescending can take a variable number of arguments in order to sort the sequence according to several properties in the order of the arguments.

For example

``` csharp
source.OrderBy("Score", "Year", "Title")
```

Is equivalent to

``` csharp
source
    .OrderBy(x => x.Score)
    .ThenBy(x => x.Year)
    .ThenBy(x => x.Title)
```

### Paging

#### PageBy

``` csharp
source.PageBy(page, pageSize)
```

Is syntactic sugar for

``` csharp
source.Skip(((page < 1 ? 1 : page) - 1) * pageSize).Take(pageSize)
```
