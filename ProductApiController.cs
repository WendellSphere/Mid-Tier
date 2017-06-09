using AppName.Web.Domain;
using AppName.Web.Domain.Account;
using AppName.Web.Models.Requests;
using AppName.Web.Models.Responses;
using AppName.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppName.Web.Controllers.Api
{
    [AllowAnonymous]
    [RoutePrefix("api/Products")]
    public class ProductApiController : ApiController
    {
        private IProductService _productService = null;
        private IUserService _userService = null;
        public ProductApiController(IProductService productService, IUserService userService)
        {
            _productService = productService; // private memeber of the instance
            _userService = userService;
        }
      
        [Route, HttpPost]
        public HttpResponseMessage Add(ProductAddRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            ItemResponse<int> response = new ItemResponse<int>();
          
            string userId = _userService.GetCurrentUserId();
          
            response.Item = _productService.Insert(model, userId);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("{id:int}"), HttpPut]
        public HttpResponseMessage Update(ProductUpdateRequest model, int id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            Product prod = _productService.Get(id);
            if (prod.Id == 0)
            {
                string error = "Product does not exist for supplied Id";
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            SuccessResponse response = new SuccessResponse();
            string currentUserId = _userService.GetCurrentUserId();
         
            _productService.Update(model, currentUserId, id);

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("{id:int}"), HttpGet]
        public HttpResponseMessage Get(int id)
        {
            Product prod = _productService.Get(id);
            if (prod.Id == 0)
            {
                string error = "Product does not exist for supplied Id";
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }
            ItemResponse<Product> response = new ItemResponse<Product>();
            response.Item = prod;
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("productType/{productType:int}"), HttpGet]
        public HttpResponseMessage GetHealthy(int productType)
        {
            ItemsResponse<Product> response = new ItemsResponse<Product>();
            response.Items = _productService.GetHealthy(productType);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
        
        [Route("{isDeleted:bool?}"), HttpGet]
        public HttpResponseMessage Get(bool isDeleted = false)
        {
            ItemsResponse<Product> response = new ItemsResponse<Product>();
            response.Items = _productService.Get(isDeleted);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("{id:int}"), HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            _productService.Delete(id);
            return Request.CreateResponse(HttpStatusCode.OK, new SuccessResponse());
        }

        [Route("configure/{LashProductId:int}/{HealthProductId:int}"), HttpDelete]
        public HttpResponseMessage Delete(int LashProductId, int HealthProductId)
        {
            _productService.Delete(LashProductId, HealthProductId);
            return Request.CreateResponse(HttpStatusCode.OK, new SuccessResponse());
        }

        [Route("configure/{LashProductId:int}/{HealthProductId:int}"), HttpPost]
        public HttpResponseMessage Insert(int LashProductId, int HealthProductId)
        {
            _productService.Insert(LashProductId, HealthProductId);
            return Request.CreateResponse(HttpStatusCode.OK, new SuccessResponse());
        }

        [Route("configure/{AccountsId}/{ProductId:int}"), HttpDelete]
        public HttpResponseMessage Delete(string AccountsId, int ProductId)
        {
            _productService.Delete(AccountsId, ProductId);
            return Request.CreateResponse(HttpStatusCode.OK, new SuccessResponse());
        }

        [Route("configure/{AccountsId}/{ProductId:int}"), HttpPost]
        public HttpResponseMessage Insert(string AccountsId, int ProductId)
        {
            _productService.Insert(AccountsId, ProductId);
            return Request.CreateResponse(HttpStatusCode.OK, new SuccessResponse());
        }
        

        [Route("addons"),HttpGet]
        public HttpResponseMessage RanPick3()
        {
            ItemsResponse<Product> response = new ItemsResponse<Product>();
            response.Items = _productService.RanPick3();
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("d/{id:int}"), HttpPut]
        public HttpResponseMessage Disable(int id)
        {
            _productService.Disable(id);
            return Request.CreateResponse(HttpStatusCode.OK, new SuccessResponse());
        }

        [Route("addons/{id:int}"), HttpGet]
        public HttpResponseMessage GetAddOns(int id)
        {
            ItemsResponse<Product> response = new ItemsResponse<Product>();
            response.Items = _productService.GetAddOns(id);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
        //get influencers by addon productId
        [Route("accounts/{id:int}"), HttpGet]
        public HttpResponseMessage GetAccounts(int id)
        {
            ItemsResponse<Account> response = new ItemsResponse<Account>();
            response.Items = _productService.GetAccounts(id);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
        
        

        [Route("getAll"),HttpGet]
        public HttpResponseMessage GetProducts()
        {
            ItemsResponse<Product> response = new ItemsResponse<Product>();
            response.Items = _productService.GetAll();
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("getAllLash"), HttpGet]
        public HttpResponseMessage GetAllLashProducts()
        {
            ItemsResponse<Product> response = new ItemsResponse<Product>();
            response.Items = _productService.GetAllLashProducts();
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("page/{pageIndex:int}/{pageSize:int}/{productType:int}"), HttpGet]
        
        public HttpResponseMessage Get(int pageIndex, int pageSize, int productType)
        {
            ItemResponse<PagedList<Product>> response = new ItemResponse<PagedList<Product>>();
            response.Item = _productService.GetProductsPaging(pageIndex, pageSize, productType);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("account/{id:int}/{pageIndex:int}/{pageSize:int}"), HttpGet]

        public HttpResponseMessage GetAccountsPag(int id, int pageIndex, int pageSize)
        {
            ItemResponse<PagedList<Account>> response = new ItemResponse<PagedList<Account>>();
            response.Item = _productService.GetAccountsPag(id, pageIndex, pageSize);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("recycled/{pageIndex:int}/{pageSize:int}"), HttpGet]

        public HttpResponseMessage GetRecycled(int pageIndex, int pageSize)
        {
            ItemResponse<PagedList<Product>> response = new ItemResponse<PagedList<Product>>();
            response.Item = _productService.GetRecycledPaging(pageIndex, pageSize);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("{search}/{pageIndex:int}/{pageSize:int}/{productType:int}"), HttpGet]

        public HttpResponseMessage GetSearchPage(string search, int pageIndex, int pageSize, int productType)
        {
            ItemResponse<PagedList<Product>> response = new ItemResponse<PagedList<Product>>();
            response.Item = _productService.Search(search, pageIndex, pageSize, productType);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }

}



