using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class Program
{
    static void Main(string[] args)
    {
        wczytywacz<Employee> em = new wczytywacz<Employee>();
        List<Employee> lEmployees = em.wczytajListe("employees.csv",
            x => new Employee(
                x[0], x[1], x[2], x[3], x[4], x[5], x[6], x[7],
                x[8], x[9], x[10], x[11], x[12], x[13], x[14],
                x[15], x[16], x[17]
            ));

        wczytywacz<EmployeeTerritory> et = new wczytywacz<EmployeeTerritory>();
        List<EmployeeTerritory> lEmployeeTerritories = et.wczytajListe("employee_territories.csv",
            x => new EmployeeTerritory(x[0], x[1]));

        wczytywacz<OrderDetail> od = new wczytywacz<OrderDetail>();
List<OrderDetail> lOrderDetails = od.wczytajListe("orders_details.csv",
    x =>
    {
        decimal.TryParse(x[2], out decimal unitPrice);
        decimal.TryParse(x[3], out decimal quantity);
        decimal.TryParse(x[4], out decimal discount);
        return new OrderDetail(x[0], x[1], unitPrice, quantity, discount);
    });


        wczytywacz<Order> o = new wczytywacz<Order>();
        List<Order> lOrders = o.wczytajListe("orders.csv",
            x => new Order(
                x[0], x[1], x[2], x[3], x[4], x[5], x[6],
                x[7], x[8], x[9], x[10], x[11], x[12], x[13]
            ));

        wczytywacz<Region> r = new wczytywacz<Region>();
        List<Region> lRegions = r.wczytajListe("regions.csv",
            x => new Region(x[0], x[1]));

        wczytywacz<Territory> t = new wczytywacz<Territory>();
        List<Territory> lTerritories = t.wczytajListe("territories.csv",
            x => new Territory(x[0], x[1], x[2]));


        foreach(var tawds in lTerritories)
        Console.WriteLine(tawds.Territorydescription);


        var nazwiskaPracownikow = from e in lEmployees
                          select e.LastName;

        var nazwiskaIRegiony = from e in lEmployees
                       join eT in lEmployeeTerritories on e.EmployeeID equals eT.Employeeid
                       join T in lTerritories on eT.Territoryid equals T.Territoryid
                       join R in lRegions on T.Regionid equals R.regionid
                       select new { e.LastName, R.regiondescription, T.Territorydescription };

        var regionyIPracownicy = from RR in lRegions
                         join T in lTerritories on RR.regionid equals T.Regionid into tGroup
                         from T in tGroup
                         join eT in lEmployeeTerritories on T.Territoryid equals eT.Territoryid into etGroup
                         from eT in etGroup
                         join e in lEmployees on eT.Employeeid equals e.EmployeeID into eGroup
                         select new { Region = RR.regiondescription, Pracownicy = eGroup.Select(e => e.LastName) };


        var liczbaPracownikowWRoznychRegionach = from R in lRegions
                                         join T in lTerritories on R.regionid equals T.Regionid into tGroup
                                         from T in tGroup
                                         join eT in lEmployeeTerritories on T.Territoryid equals eT.Territoryid into etGroup
                                         select new { Region = R.regiondescription, LiczbaPracownikow = etGroup.Select(et => et.Employeeid).Distinct().Count() };


                var ordersWithDetails = from order in lOrders
                                join orderDetail in lOrderDetails
                                on order.Orderid equals orderDetail.OrderId
                                select new { Order = order, OrderDetail = orderDetail };

        // Grupuj zamówienia po pracownikach i wykonaj odpowiednie operacje
        var wyniki = from order in ordersWithDetails
                     group order by order.Order.Employeeid into g
                     select new
                     {
                         EmployeeID = g.Key,
                         LiczbaZamowien = g.Select(x => x.Order).Distinct().Count(),
                         SredniaWartoscZamowienia = g.Average(x => x.OrderDetail.Unitprice * x.OrderDetail.Quantity * (1 - (decimal)x.OrderDetail.Discount)),
                         MaksymalnaWartoscZamowienia = g.Max(x => x.OrderDetail.Unitprice * x.OrderDetail.Quantity * (1 - (decimal)x.OrderDetail.Discount))


                     };
                // 2. [1 punkt] wybierz nazwiska wszystkich pracowników.
        Console.WriteLine("2. [1 punkt] wybierz nazwiska wszystkich pracowników.");
        foreach (var item in nazwiskaPracownikow)
        {
            Console.WriteLine(item);
        }

        // [1 punkt] wypisz nazwiska pracowników oraz dla każdego z nich nazwę regionu i terytorium gdzie pracuje. Rezultatem kwerendy LINQ będzie "płaska" lista, więc nazwiska mogą się powtarzać (ale każdy rekord będzie unikalny).
        Console.WriteLine("// [1 punkt] wypisz nazwiska pracowników oraz dla każdego z nich nazwę regionu i terytorium gdzie pracuje. Rezultatem kwerendy LINQ będzie płaska lista, więc nazwiska mogą się powtarzać (ale każdy rekord będzie unikalny).");
        foreach (var item in nazwiskaIRegiony)
        {
            Console.WriteLine($"{item.LastName}, {item.regiondescription}, {item.Territorydescription}");
        }

        // wypisz nazwy regionów oraz nazwiska pracowników, którzy pracują w tych regionach, pracownicy mają być zagregowani po regionach, rezultatem ma być lista regionów z podlistą pracowników (odpowiednik groupjoin).
        Console.WriteLine("// wypisz nazwy regionów oraz nazwiska pracowników, którzy pracują w tych regionach, pracownicy mają być zagregowani po regionach, rezultatem ma być lista regionów z podlistą pracowników (odpowiednik groupjoin).");
        foreach (var item in regionyIPracownicy)
        {
            Console.WriteLine($"{item.Region}: {string.Join(", ", item.Pracownicy)}");
        }
        Console.WriteLine("wypisz nazwy regionów oraz liczbę pracowników w tych regionach.");
        // wypisz nazwy regionów oraz liczbę pracowników w tych regionach.
        foreach (var item in liczbaPracownikowWRoznychRegionach)
        {
            Console.WriteLine($"{item.Region}: {item.LiczbaPracownikow}");
        }

        // wczytaj do odpowiednich struktur dane z plików orders.csv oraz orders_details.csv. Następnie dla każdego pracownika wypisz liczbę dokonanych przez niego zamówień, średnią wartość zamówienia oraz maksymalną wartość zamówienia.
        Console.WriteLine("// wczytaj do odpowiednich struktur dane z plików orders.csv oraz orders_details.csv. Następnie dla każdego pracownika wypisz liczbę dokonanych przez niego zamówień, średnią wartość zamówienia oraz maksymalną wartość zamówienia.");
        foreach (var wynik in wyniki)
        {
            Console.WriteLine($"Pracownik {wynik.EmployeeID}:");
            Console.WriteLine($"Liczba zamówień: {wynik.LiczbaZamowien}");
            Console.WriteLine($"Średnia wartość zamówienia: {wynik.SredniaWartoscZamowienia:C}");
            Console.WriteLine($"Maksymalna wartość zamówienia: {wynik.MaksymalnaWartoscZamowienia:C}");
            Console.WriteLine();
        }
    }
}


class wczytywacz<T>
{
    public List<T> wczytajListe(string path, Func<string[], T> generuj)
    {
        List<T> lista = new List<T>();
        using (var sr = new StreamReader(path))
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(',');
                lista.Add(generuj(values));
            }
        }
        return lista;
    }
}

class Employee
{
    public string EmployeeID { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string Title { get; set; }
    public string TitleOfCourtesy { get; set; }
    public string BirthDate { get; set; }
    public string HireDate { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string HomePhone { get; set; }
    public string Extension { get; set; }
    public string Photo { get; set; }
    public string Notes { get; set; }
    public string ReportsTo { get; set; }
    public string PhotoPath { get; set; }

    public Employee(
        string employeeID, string lastName, string firstName, string title,
        string titleOfCourtesy, string birthDate, string hireDate, string address,
        string city, string region, string postalCode, string country, string homePhone,
        string extension, string photo, string notes,string reportsTo,string photoPath)
            {
                EmployeeID = employeeID;
                LastName = lastName;
                FirstName = firstName;
                Title = title;
                TitleOfCourtesy = titleOfCourtesy;
                BirthDate = birthDate;
                HireDate = hireDate;
                Address = address;
                City = city;
                Region = region;
                PostalCode = postalCode;
                Country = country;
                HomePhone = homePhone;
                Extension = extension;
                Photo = photo;
                Notes = notes;
                ReportsTo = reportsTo;
                PhotoPath = photoPath;
    }
}


    class EmployeeTerritory
    {
        public EmployeeTerritory(string employeeid, string territoryid)
        {
            Employeeid = employeeid;
            Territoryid = territoryid;
        }
        public string Employeeid {get; set;}
        public string Territoryid {get; set;}
    }

    class Order
{
    public String Orderid {get; set;}
    public String Customerid {get; set;}
    public String Employeeid {get; set;}
    public String Orderdate {get; set;}
    public String Requireddate {get; set;}
    public String Shippeddate {get; set;}
    public String Shipvia {get; set;}
    public String Freight {get; set;}
    public String Shipname {get; set;}
    public String Shipaddress {get; set;}
    public String Shipcity {get; set;}
    public String Shipregion {get; set;}
    public String Shippostalcode {get; set;}
    public String Shipcountry {get; set;}

    public Order(string orderid, string customerid, string employeeid, string orderdate, string requireddate, string shippeddate, string shipvia, string freight, string shipname, string shipaddress, string shipcity, string shipregion, string shippostalcode, string shipcountry)
    {
        Orderid = orderid;
        Customerid = customerid;
        Employeeid = employeeid;
        Orderdate = orderdate;
        Requireddate = requireddate;
        Shippeddate = shippeddate;
        Shipvia = shipvia;
        Freight = freight;
        Shipname = shipname;
        Shipaddress = shipaddress;
        Shipcity = shipcity;
        Shipregion = shipregion;
        Shippostalcode = shippostalcode;
        Shipcountry = shipcountry;
    }
}

    class OrderDetail
    {
        public String OrderId  {get; set;}
        public String ProductId {get; set;}
        public decimal Unitprice {get; set;}
        public decimal Quantity {get; set;}
        public decimal Discount {get; set;}

        public OrderDetail(String orderId, String productId, decimal unitprice, decimal quantity, decimal discount)
        {
            OrderId = orderId;
            ProductId = productId;
            Unitprice = unitprice;
            Quantity = quantity;
            Discount = discount;
        }
    }

        class Region
    {
        public Region(string regionid, string regiondescription)
        {
            this.regionid = regionid;
            this.regiondescription = regiondescription;
        }

        public string regionid {get; set;}
        public string regiondescription {get; set;}
    }

        class Territory
    {
        public Territory(string territoryid, string territorydescription, string regionid)
        {
            Territoryid = territoryid;
            Territorydescription = territorydescription;
            Regionid = regionid;
        }

        public string Territoryid {get; set;}
        public string Territorydescription {get; set;}
        public string Regionid {get; set;}
    }