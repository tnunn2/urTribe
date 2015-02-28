﻿using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using urTribeWebAPI;

namespace urTribeWindowService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("Starting service...");
            string uri = "http://ec2-54-68-190-116.us-west-2.compute.amazonaws.com:8081";
            WebApp.Start<Startup>(uri);
        }

        protected override void OnStop()
        {
            Console.WriteLine("Stopping service...");
        }
    }
}
