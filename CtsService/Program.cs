﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CtsService
{
	static class Program
	{

		/// <summary>
		/// The main entry point for the apphlication.
		/// </summary>
		static void Main()
		{ 
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new CtsService()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
