using AppName.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppName.Web.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("products")]
    public class ProductsController : SiteController
    {
        public override string BodyPageCss
        {
            get
            {
                return "product-page";
            }
        }

        public override OwnerType OwnerType
        {
            get
            {
                return OwnerType.Product;
            }
        }

        public override string EntityId
        {
            get
            {
                return _entityId;
            }
        }

        [Route]
        public ActionResult List()
        {
            ViewBag.Message = "Your product page.";

            return View();
        }

        [Route("detail/{id:int}")]
        public ActionResult ProductDetail(int id)
        {
            _entityId = id.ToString();
            ItemViewModel<int> vm = GetViewModel<ItemViewModel<int>>();
            vm.Item = id;
            return View(vm);
        }
    }
}