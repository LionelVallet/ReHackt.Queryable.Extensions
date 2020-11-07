# ReHackt.Queryable.Extensions
Some useful System.Linq.IQueryable extensions such as filtering, ordering, paging...

## QueryableFilter

`QueryableFilter<T>` allows to dynamically filter an `IQueryable<T>` with a query string.

For example

``` csharp

```

Is equivalent to

``` csharp

```

### Supported in query

* Boolean operators: **and**, **or**
* Comparison operators: **eq**, **gt**, **gte**, **lt**, **lte**, **in**
* Value types: **bool**, **DateTime**, **double**, **int**, **string**
* **Parentheses**
* **Property names**

### Not yet supported (planned)

* Boolean operators: **not**
* Value types: **enum**

## IQueryable extensions

### Filtering

#### Filter

`// Documentation in progress`

#### WhereIf

``` csharp
source.WhereIf(condition, predicate)
```

is syntactic sugar for

``` csharp
condition ? source.Where(predicate) : source
```

This allows you to keep the LINQ method syntax to apply filters according to a condition that does not depend on the element being tested.

For example

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

Is equivalent to

``` csharp
return source
           .Join(...)
           .Where(...)
           .WhereIf(condition1, predicate1)
           .WhereIf(condition2, predicate2)
           .OrderBy(...)
           .Select(...);
```

### Ordering

`// Documentation in progress`

### Paging

#### PageBy

``` csharp
source.PageBy(page, pageSize)
```

is syntactic sugar for

``` csharp
source.Skip(((page < 1 ? 1 : page) - 1) * pageSize).Take(pageSize)
```
