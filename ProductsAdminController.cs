using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using AppName.Web.Domain;
using AppName.Web;
using AppName.Web.Services;
using AppName.Web.Models.ViewModels;
using AppName.Web.Services.Interfaces;
using AppName.Web.Controllers.BaseControllers;

namespace AppName.Web.Controllers
{
    
    [RoutePrefix("Products")]
    public class ProductsAdminController : AdminBaseController
    {
        IAccountsService _accountService = null;

        public ProductsAdminController(IAccountsService accountService)
        {
            _accountService = accountService;
        }

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
        
        [Route]
        public ActionResult list()
        {
            return View();
        }

        [Route("{id:int}/configure")]
        public ActionResult configure(int id = 0)
        {
            ItemViewModel<int> vm = GetViewModel<ItemViewModel<int>>();
            vm.Item = id; 
            return View(vm);
        }

       
        [Route("list")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("jq")]
        public ActionResult Jq()
        {
            return View();
        }
       
        [Route("{id:int}/Edit")]
        [Route("Create")]
        public ActionResult Edit(int id = 0)
        {
            ItemViewModel<int> vm = GetViewModel<ItemViewModel<int>>();
            vm.Item = id;
            return View(vm);
        }
      
        [Route("DetailOld")]
        public ActionResult Public()
        {
            return View();
        }

       
        [Route("PublicEx")]
        public ActionResult PublicEx()
        {
            return View();
        }

        [Route("{id:int}/images")]
        public ActionResult ProductImages(int id)
        {
            ItemViewModel<int> model = GetViewModel<ItemViewModel<int>>();
            model.Item = id;
            return View(model);
        }

        [Route("Dashboard")]
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}
