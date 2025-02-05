﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using petStoreMonitoringApp.Models;

public class petStoreMonitoringAppContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public petStoreMonitoringAppContext(DbContextOptions<petStoreMonitoringAppContext> options)
        : base(options)
    {
    }
    
    public DbSet<petStoreMonitoringApp.Models.OnHandMerch> OnHandMerch { get; set; }
    public DbSet<petStoreMonitoringApp.Models.AnimalPurchaseCategory> AnimalPurchaseCategory { get; set; }
    public DbSet<petStoreMonitoringApp.Models.OrderState> OrderState { get; set; }
    public DbSet<petStoreMonitoringApp.Models.Session> Session { get; set; }
}