﻿using ArtShop.Data.Model;
using ArtShop.Data.Services;
using OdeToFood.WebSite.Controllers;
using OdeToFood.WebSite.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ArtShop.WebSite.Controllers
{
    public class ProductController : BaseController
    {

        readonly BaseDataService<Product> db;
        readonly BaseDataService<Artist> dbArtist ;

        public ProductController()
        {
            db = new BaseDataService<Product>();
            dbArtist = new BaseDataService<Artist>();
        }
        public ActionResult Index()
        {
            var model = db.Get().OrderBy(x=>x.Id);
            return View(model);
        }
        public ActionResult itemProduct(int? id)
        {
            if (id == null)
            {
                Logger.Instance.LogException(new Exception("Id Product null "));
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.GetById(id.Value);
            if (product == null)
            {
                Logger.Instance.LogException(new Exception("Product HttpNotFound"));
                return HttpNotFound();
            }
            return View(product);
            
        }
        [HttpPost]
        public ActionResult itemProduct(Product product)
        {
            return View(product);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult ABMView()
        {
            var list = db.Get();
            foreach(var item in list)
            {
                item.Artista = dbArtist.GetById(Convert.ToInt32(item.ArtistID)); 
            }
            return View(list);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            


            //List<SelectListItem> listItems = new List<SelectListItem>();
            //foreach (var item in artistas)
            //{
            //    listItems.Add(new SelectListItem() { Text = item.FullName, Value = item.Id.ToString() });
            //}
            //ViewBag.artistas = listItems;

            Product model = new Product()
            {
                Artistas = dbArtist.Get()
            };
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create(Product pintura, HttpPostedFileBase file)
        {
            
            if (ModelState.IsValid)
            {   
                try
                {
                    if (file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string path = Path.Combine(Server.MapPath("~/Content/images/Paints"), fileName);
                        file.SaveAs(path);
                        pintura.Image = file.FileName;
                        this.CheckAuditPattern(pintura, true);
                        var list = db.ValidateModel(pintura);
                        db.Create(pintura);
                        return RedirectToAction("ABMView");
                    }

                }
                catch (System.Exception ex)
                {
                    Logger.Instance.LogException(ex);
                    ViewBag.MessageDanger = ex.Message;
                    return View(pintura);
                }
            }
        
            ViewBag.MessageDanger = "Error al cargar el archivo";
            return View(pintura);
        }

        public ActionResult Edit(int? id)
        {
            BaseDataService<Artist> dbArtist = new BaseDataService<Artist>();
            if (id == null)
            {
                Logger.Instance.LogException(new Exception("Id Paint null "));
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var pintura = db.GetById(id.Value);
            if (pintura == null)
            {
                Logger.Instance.LogException(new Exception("Paint HttpNotFound"));
                return HttpNotFound();
            }
            pintura.Artistas = dbArtist.Get();
            return View(pintura);
        }
        [HttpPost]
        public ActionResult Edit(Product pintura,HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (file != null)
                    {
                        if (file.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(file.FileName);
                            string path = Path.Combine(Server.MapPath("~/Content/Images/Paints"), fileName);
                            file.SaveAs(path);
                            pintura.Image = file.FileName;

                            this.CheckAuditPattern(pintura);
                            var list = db.ValidateModel(pintura);

                            db.Update(pintura);
                            return RedirectToAction("ABMView");
                        }
                    }
                    
                    else
                    {
                        this.CheckAuditPattern(pintura);
                        var list = db.ValidateModel(pintura);

                        db.Update(pintura);
                        return RedirectToAction("ABMView");

                    }
                   

                }
                catch (Exception ex)
                {
                    Logger.Instance.LogException(ex);
                    ViewBag.MessageDanger = ex.Message;
                    return View(pintura);
                }
            }
            return View(pintura);

        }

        
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var pintura = db.GetById(id.Value);
            if (pintura == null)
            {
                Logger.Instance.LogException(new Exception("Paint HttpNotFound"));
                return HttpNotFound();
            }
            try
            {
                db.Delete(pintura);
                return RedirectToAction("ABMView");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex);
                ViewBag.MessageDanger = ex.Message;
                return RedirectToAction("ABMView");
            }

        }
    }
}