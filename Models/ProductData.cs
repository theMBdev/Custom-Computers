using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using CustomComputersGU.Models.Poco;
using CustomComputersGU.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace CustomComputersGU.Models
{

    // This is where i seed my data

    public class ProductData : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        protected override void Seed(ApplicationDbContext context)
        {
            // Seeds customer role (other roles are created in accountcontroller)
            if (!context.Roles.Any(r => r.Name == "Customer"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "Customer" };

                manager.Create(role);
            }
            // Seeds categories
            var categories = new List<Category>
            {
                new Category { Name = "Processors", ArtUrl = "/Content/Images/cpu.png", Description = "Installing a faster processor is one of the most effective ways of improving the performance of a computer. The processor is a microchip that handles instructions and commands. It processes this information and the faster it can do this means the quicker and smoother the computer will perform and the more complex tasks it can carry out."},
                new Category { Name = "Memory", ArtUrl = "/Content/Images/memory.png", Description = "Desktop PC memory stores the programmes and applications that are running on a computer including the operating system. Referred to as RAM (Random Access Memory) and installed in the PC as modules typically in banks of 4 or 8GB the more memory a computer has the more easily it can cope with multitasking." },
                new Category { Name = "Graphics Cards", ArtUrl = "/Content/Images/gpu.png", Description = "The CPU may be the brain of your PC, but when it comes to gaming, the graphics card is the beating heart that pumps pixels out of your obelisk of a tower and into your monitor. A graphics card consists of dedicated video memory and a graphics processing unit (GPU) that handles all sorts of calculations, like mapping textures and lighting surfaces while rendering millions of polygons." },
                new Category { Name = "Power Supplies", ArtUrl = "/Content/Images/power.png", Description = "The role of a PC power supply unit is to convert the AC electric power that comes from the mains to the DC power that the computer requires. But it can do much more than that. A good quality power supply can make your system more efficient, stable and reliable." },
                new Category { Name = "Storage", ArtUrl = "/Content/Images/hdd.png", Description = "A Hard Drive (HDD – Hard Disk Drive) is the main data storage device used with a Personal Computer. The Hard Drive stores all the programmes, applications and documents used by the computer including the operating system and, as such, requires a large capacity usually in excess of 1TB, though many PCs are supplied with a stock 500GB Hard Drive." },
            };
            
            // Seeds products
            new List<Product>
            {
                //Graphics Gards
                new Product { Name = "GTX 1080", Category = categories.Single(g => g.Name == "Graphics Cards"), Description = "The GeForce GTX 1080 is the high-range model from the GTX 10- series of graphics cards. The GeForce GTX 1080 is VR Ready.", Price = 500.00M, ArtUrl = "/Content/Images/gtx1080.jpg", UnitsInStock = 7},
                new Product { Name = "GTX 1070", Category = categories.Single(g => g.Name == "Graphics Cards"), Description = "The GeForce GTX 1070 is the mid/high-range model from the GTX 10- series of graphics cards. The GeForce GTX 1070 is VR Ready.",Price = 350.00M, ArtUrl = "/Content/Images/gtx1070.jpg", UnitsInStock = 7 },
                new Product { Name = "GTX 1060", Category = categories.Single(g => g.Name == "Graphics Cards"), Description = "The GeForce GTX 1060 is the mid-range model from the GTX 10- series of graphics cards and is available with 3 or 6 GB of VRAM. The GeForce GTX 1060 is VR Ready and comes with an HDMI 2.0b port.",Price = 210.00M, ArtUrl = "/Content/Images/gtx1060.jpg", UnitsInStock = 7 },

                //Processors
                new Product { Name = "Intel Core i5-6600K 3.9GHz", Category = categories.Single(g => g.Name == "Processors"), Description="The Intel® Core™ i5 6600K boasts a 3.5 GHz base frequency and 3.9 GHz turbo frequency, the processor is also built on a 14 nm process, with a 95 W maximum TDP. The chip is a Quad-Core design with 4 threads.", Price = 200.00M, ArtUrl = "/Content/Images/i5.jpg", UnitsInStock = 7 },
                new Product { Name = "Intel Core i7-6700K 4.0GHz ", Category = categories.Single(g => g.Name == "Processors"), Description="The Intel® Core™ i7 6700K boasts a 4.0 GHz base frequency and 4.2 GHz turbo frequency, the processor is also built on a 14 nm process, with a 95 W maximum TDP. The chip is a Quad-Core design with 8 threads. ", Price = 350.00M, ArtUrl = "/Content/Images/i7.jpg", UnitsInStock = 7 },

                //Memory
                new Product { Name = "Fury Black 16GB (4x4GB) DDR4 ", Category = categories.Single(g => g.Name == "Memory"), Description="Kingston Fury DDR4 is an aggressively styled, low profile memory, optimised for intels 200 series and X99 chipset. Featuring many advances in memory technology including PnP, XMP 2.0 and specially designed asymmetrical heat spreaders keeping the memory cool even when pushed to the max.", Price = 140.00M, ArtUrl = "/Content/Images/ram2.jpg", UnitsInStock = 7 },
                new Product { Name = "Fury Black 8GB (2x4GB) DDR4", Category = categories.Single(g => g.Name == "Memory"), Description="Kingston Fury DDR4 is an aggressively styled, low profile memory, optimised for intels 200 series and X99 chipset. Featuring many advances in memory technology including PnP, XMP 2.0 and specially designed asymmetrical heat spreaders keeping the memory cool even when pushed to the max.", Price = 70.00M, ArtUrl = "/Content/Images/ram4.jpg", UnitsInStock = 7 },

                //Power Supplies
                new Product { Name = "Corsair VS Series 550 Watt", Category = categories.Single(g => g.Name == "Power Supplies"), Description="The VS550 delivers a guaranteed 550 Watts of continuous power. With universal input the power supply includes circuitry with components that can tolerate a wide range of mains voltage support (100-240 VAC). Multiple power protection circuits provide for added peace of mind.", Price = 40.00M, ArtUrl = "/Content/Images/psu550.jpg", UnitsInStock = 7 },
                new Product { Name = "Corsair VS Series 650 Watt", Category = categories.Single(g => g.Name == "Power Supplies"), Description="The VS650 delivers a guaranteed 650 Watts of continuous power. With universal input the power supply includes circuitry with components that can tolerate a wide range of mains voltage support (100-240 VAC). Multiple power protection circuits provide for added peace of mind.",Price = 55.00M, ArtUrl = "/Content/Images/psu650.jpg", UnitsInStock = 7 },

                //Storage
                new Product { Name = "500GB 850 EVO SSD 2.5 SATA", Category = categories.Single(g => g.Name == "Storage"), Description="The 850 EVO is the advanced consumer SSD powered by 3D V-NAND technology that maximizes everyday computing experiences with optimized performance and enhanced reliability.", Price = 180.00M, ArtUrl = "/Content/Images/samevo.jpg", UnitsInStock = 7 },
                new Product { Name = "250GB 850 EVO SSD 2.5 SATA", Category = categories.Single(g => g.Name == "Storage"), Description="The 850 EVO is the advanced consumer SSD powered by 3D V-NAND technology that maximizes everyday computing experiences with optimized performance and enhanced reliability.", Price = 90.00M, ArtUrl = "/Content/Images/samevo.jpg", UnitsInStock = 7 },


            }.ForEach(a => context.Products.Add(a));


            //if (!context.Roles.Any(r => r.Name == "StoreManager"))
            //{
            //    var store = new RoleStore<IdentityRole>(context);
            //    var manager = new RoleManager<IdentityRole>(store);
            //    var role = new IdentityRole { Name = "StoreManager" };

            //    manager.Create(role);
            //}

            //if (!context.Roles.Any(r => r.Name == "StoresManager"))
            //{
            //    var store = new RoleStore<IdentityRole>(context);
            //    var manager = new RoleManager<IdentityRole>(store);
            //    var role = new IdentityRole { Name = "StoresManager" };

            //    manager.Create(role);
            //}

            //if (!context.Roles.Any(r => r.Name == "AssistanManager"))
            //{
            //    var store = new RoleStore<IdentityRole>(context);
            //    var manager = new RoleManager<IdentityRole>(store);
            //    var role = new IdentityRole { Name = "AssistanManager" };

            //    manager.Create(role);
            //}

            //if (!context.Roles.Any(r => r.Name == "InvoiceClerk"))
            //{
            //    var store = new RoleStore<IdentityRole>(context);
            //    var manager = new RoleManager<IdentityRole>(store);
            //    var role = new IdentityRole { Name = "InvoiceClerk" };

            //    manager.Create(role);
            //}

            //if (!context.Roles.Any(r => r.Name == "SalesAssistant"))
            //{
            //    var store = new RoleStore<IdentityRole>(context);
            //    var manager = new RoleManager<IdentityRole>(store);
            //    var role = new IdentityRole { Name = "SalesAssistant" };

            //    manager.Create(role);
            //}

        }
    }
}