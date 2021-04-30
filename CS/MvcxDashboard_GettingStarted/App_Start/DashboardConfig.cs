using System.Web.Routing;
using System.Xml.Linq;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DashboardWeb.Mvc;
using DevExpress.DataAccess;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.MongoDB;

namespace MvcxDashboard_GettingStarted {
    public class DashboardConfig {
        public static void RegisterService(RouteCollection routes) {
            routes.MapDashboardRoute();

            // Uncomment this line to save dashboards to the App_Data folder.
            DashboardConfigurator.Default.SetDashboardStorage(new DashboardFileStorage(@"~/App_Data/Dashboards"));

            // Uncomment these lines to create an in-memory storage of dashboard data sources. Use the DataSourceInMemoryStorage.RegisterDataSource
            // method to register the existing data source in the created storage.
            //var dataSourceStorage = new DataSourceInMemoryStorage();
            //DashboardConfigurator.Default.SetDataSourceStorage(dataSourceStorage);

            DashboardConfigurator.Default.SetConnectionStringsProvider(new DevExpress.DataAccess.Web.ConfigFileConnectionStringsProvider());
            DashboardConfigurator.Default.SetDataSourceStorage(CreateDataSourceStorage());

            DashboardConfigurator.Default.ConfigureDataConnection += DefaultOnConfigureDataConnection;
        }

        static void DefaultOnConfigureDataConnection(object sender, ConfigureDataConnectionWebEventArgs e) {
            if(e.ConnectionName == "mongoDataSourceConnection") {
                // MongoDB without authentication credentials.
                e.ConnectionParameters = new MongoDBConnectionParameters("dataaccessdbxs", false, 27017);
            }

            if(e.ConnectionName == "Northwind") {
                e.ConnectionParameters = new MsSqlConnectionParameters("dataaccessdbxs", "Northwind", "sa", "dx", MsSqlAuthorizationType.SqlServer);
            }
        }

        static DataSourceInMemoryStorage CreateDataSourceStorage() {
            var dataSourceStorage = new DataSourceInMemoryStorage();

            DashboardMongoDBDataSource mongoDataSource = new DashboardMongoDBDataSource("Mongo DB with Connection String", "mongoDataSourceConnection");
            MongoDBQuery queryProductsFiltered = new MongoDBQuery() {
                DatabaseName = "Northwind",
                CollectionName = "Products",
                FilterString = "[CategoryID] = ?CategoryID and [UnitPrice] > 30",
                Alias = "Filtered Products"
            };
            var queryParam = new DevExpress.DataAccess.MongoDB.QueryParameter("CategoryID", typeof(Expression), new Expression("?CategoryID", typeof(int)));
            queryProductsFiltered.Parameters.Add(queryParam);

            mongoDataSource.Queries.Add(queryProductsFiltered);
            XElement mongoXml = mongoDataSource.SaveToXml();
            dataSourceStorage.RegisterDataSource("mongoDataSource", mongoXml);

            return dataSourceStorage;
        }
    }
}