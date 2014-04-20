﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.PageObjects;
using System.Text.RegularExpressions;

namespace AI_2048
{
    /// <summary>
    /// Selenium Page Object for the game itself
    /// </summary>
    public class GamePage : IDisposable
    {
        #region Constants
        /// <summary>
        /// URL of the game
        /// </summary>
        public static string GAME_SITE_URL = "http://gabrielecirulli.github.io/2048/";

        /// <summary>
        /// Relative path to chromedriver
        /// </summary>
        private static string CHROME_DRIVER_PATH = @"packages\Selenium.WebDriver.ChromeDriver.2.9.0.1\content";
        #endregion

        #region Page Object
        // <summary>
        /// Selenium web driver
        /// </summary>
        private IWebDriver Driver { get; set; }

        /// <summary>
        /// Page body, used for keypress
        /// </summary>
        [FindsBy(How = How.TagName, Using = "body")]
        private IWebElement PageBody { get; set; }

        /// <summary>
        /// Selenium web driver JS executor
        /// </summary>
        private IJavaScriptExecutor JavaScript
        {
            get
            {
                return Driver as IJavaScriptExecutor;
            }
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Default GamePage Constructor
        /// </summary>
        /// <param name="driver">Web driver to use</param>
        public GamePage()
        {
            Driver = new ChromeDriver(CHROME_DRIVER_PATH);
            Driver.Navigate().GoToUrl(GAME_SITE_URL);
            PageFactory.InitElements(Driver, this);
            JSInject();
        }

        /// <summary>
        /// Gets the current game score
        /// </summary>
        /// <returns>Current score</returns>
        public int GetScore()
        {
            return (int)JavaScript.ExecuteScript("return GameManager._instance.score");
        }

        /// <summary>
        /// Read the current game state
        /// </summary>
        public long[,] GetGameState()
        {
            long[,] grid = new long[4, 4];
            var cells = (ReadOnlyCollection<object>)JavaScript.ExecuteScript("return GameManager._instance.grid.cells");
            for (int i = 0; i < 4; ++i)
            {
                var col = cells[i] as ReadOnlyCollection<object>;
                for(int j = 0; j < 4; ++j)
                {
                    if(col[j] != null)
                    {
                        var cell = col[j] as Dictionary<string, object>;
                        grid[j, i] = (long)cell["value"];
                    }
                    else
                    {
                        grid[j, i] = 0;
                    }
                }
            }
            return grid;
        }

        /// <summary>
        /// Make a move in the game
        /// </summary>
        /// <param name="key">Key to press</param>
        public void MakeMove(Moves move)
        {
            JavaScript.ExecuteScript(string.Format("GameManager._instance.move({0})", (int)move));
        }

        /// <summary>
        /// Dispose of game page
        /// </summary>
        public void Dispose()
        {
            Driver.Quit();
        }

        /// <summary>
        /// Inject some JS code into the browser so we can control the game
        /// </summary>
        private void JSInject()
        {
            var funcTmp = JavaScript.ExecuteScript("return GameManager.prototype.isGameTerminated.toString();");
            JavaScript.ExecuteScript("GameManager.prototype.isGameTerminated = function() { GameManager._instance = this; return true; }");
            PageBody.SendKeys(Keys.ArrowUp);
            JavaScript.ExecuteScript(string.Format("eval(GameManager.prototype.isGameTerminated = {0})", funcTmp));
        }
        #endregion
    }
}
