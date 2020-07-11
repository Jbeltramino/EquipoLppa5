﻿using ArtShop.Data.Model;
using ArtShop.Data.Services;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using OdeToFood.WebSite.Controllers;
using OdeToFood.WebSite.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ArtShop.WebSite.Controllers
{
    public class CartController : BaseController
    {
        readonly BaseDataService<CartItem> dbItem;
        readonly BaseDataService<Cart> db;
        readonly BaseDataService<Product> dbProduct;

        public CartController()
        {
            db = new BaseDataService<Cart>();
            dbItem = new BaseDataService<CartItem>();
            dbProduct = new BaseDataService<Product>();
        }
        // GET: Cart
        public ActionResult Index()
        {
            if (Request.Cookies["cookieCart"] != null)
            {
                HttpCookie cookie = HttpContext.Request.Cookies.Get("cookieCart");

                List<CartItem> listaItems = JsonConvert.DeserializeObject<List<CartItem>>(cookie.Value);

                return View(listaItems);
            }
            return RedirectToAction("index", "Home");

        }
        [Authorize]
        [HttpPost]
        public ActionResult AddToCart(int? Id)
        {
            Product oPaint = dbProduct.GetById(Convert.ToInt32(Id));
            if (Id == null)
            {
                Logger.Instance.LogException(new Exception("Id Cart null "), User.Identity.GetUserId());
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.Cookies.Get("cookieCart")==null)
            {
                
                Cart oCart = new Cart();

                List<CartItem> items = new List<CartItem>();
                CartItem oCartItem =new CartItem()
                {
                    ProductId = Convert.ToInt32(Id),
                    Price = oPaint.Price,
                    Quantity = 1,
                    _Product = oPaint
                };
                

                //Seteo datos de cart
                oCart.Cookie ="XX"; //ARREGLAR ESTO
                oCart.ItemCount = 1;
                var format = "dd/MM/yyyy HH:mm:ss";
                var hoy = DateTime.Now.ToString(format);
                var dateTime = DateTime.ParseExact(hoy, format, CultureInfo.InvariantCulture);
                oCart.CartDate = dateTime;

               
                
                    
                this.CheckAuditPattern(oCart, true);
                var list = db.ValidateModel(oCart);

                if (ModelIsValid(list))
                    return RedirectToAction("itemProduct", "Product", new { Id });
                try
                {
                    //Obtengo el id del carritoCreado
                    Cart oCartSave = db.Create(oCart);
                    oCartItem.CartId = oCartSave.Id;
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogException(ex, User.Identity.GetUserId());

                }

                this.CheckAuditPattern(oCartItem, true);
                var list2 = dbItem.ValidateModel(oCartItem);

               

                


                if (ModelIsValid(list2))
                    return RedirectToAction("itemProduct", "Product", new { Id });
                try
                {
                    //Guardo el item
                    CartItem ItemCartSave= dbItem.Create(oCartItem);


                     items.Add(ItemCartSave);
                    //Guardo la lista en json en la cookie
                    HttpCookie cookie = new HttpCookie("cookieCart");
                    string jsonItems = JsonConvert.SerializeObject(items);
                    cookie.Value = jsonItems;
                    this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogException(ex, User.Identity.GetUserId());
                }
                    
               
               
                
            }
            else
            {
                HttpCookie cookie = HttpContext.Request.Cookies.Get("cookieCart");
                List<CartItem> listaItems = JsonConvert.DeserializeObject<List<CartItem>>(cookie.Value);

                //Saco el id del carrito del primer item
                int idCart = listaItems[0].CartId;


                CartItem oCartItem = new CartItem()
                {
                    ProductId = Convert.ToInt32(Id),
                    Price = oPaint.Price,
                    Quantity = 1,
                    CartId=idCart,
                    _Product = oPaint
                };
                

                //DEBERIA ACTUALIZAR LA COOKIE DEL CART SI NO SE GUARDA EL NOMBRE



                this.CheckAuditPattern(oCartItem, true);
                var list2 = dbItem.ValidateModel(oCartItem);


              

                if (ModelIsValid(list2))
                    return RedirectToAction("itemProduct", "Product", new { Id });
                try
                {
                    //Guardo el item
                    CartItem oCartItemSave = dbItem.Create(oCartItem);

                    listaItems.Add(oCartItemSave);
                    string jsonItems = JsonConvert.SerializeObject(listaItems);
                    cookie.Value = jsonItems;
                    this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);

                    var precio = 0.0;
                    foreach(var item in listaItems)
                    {
                        precio = precio + item.Price;
                        
                    }

                }
                catch (Exception ex)
                {
                    Logger.Instance.LogException(ex, User.Identity.GetUserId());
                }

            }

            return RedirectToAction("itemProduct", "Product", new { Id });
        }

        public ActionResult getPrice()
        {
            HttpCookie cookie = HttpContext.Request.Cookies.Get("cookieCart");
            var precio = 0.0;
            if (cookie != null)
            {
                List<CartItem> listaItems = JsonConvert.DeserializeObject<List<CartItem>>(cookie.Value);

                foreach (var item in listaItems)
                {
                    precio = precio + item.Price;

                }
            }
            
            return Json(new { response = precio }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult deleteCartItems()
        {
            
            if (Request.Cookies["cookieCart"] != null)
            {

                Response.Cookies["cookieCart"].Expires = DateTime.Now.AddDays(-1);

                HttpCookie cookie = HttpContext.Request.Cookies.Get("cookieCart");

                List<CartItem> listaItems = JsonConvert.DeserializeObject<List<CartItem>>(cookie.Value);

                foreach (var item in listaItems)
                {
                    dbItem.Delete(item);

                }
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

    }
}