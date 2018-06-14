using System;
using System.Collections.Generic;
using SpringHeroBank.entity;
using SpringHeroBank.model;
using SpringHeroBank.utility;
using SpringHeroBank.view;

namespace SpringHeroBank
{
    class Program
    {
        public static Account currentLoggedIn;
        
        static void Main(string[] args)
        {
            MainView.GenerateMenu();           
        }
    }
}