using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Bootstrap {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route("Admin/" + Constants.RoutesAreaName,
                        new RouteValueDictionary {
                            {"area", Constants.RoutesAreaName},
                            {"controller", "Admin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", Constants.RoutesAreaName}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}
