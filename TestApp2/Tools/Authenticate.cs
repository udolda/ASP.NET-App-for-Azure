using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace TestApp2.Tools
{

    public class Authenticate
    {
        private TfsTeamProjectCollection teamProjectCollection;
        public Authenticate()
        {
            teamProjectCollection = null;
        }

        public TfsTeamProjectCollection PerformAuthentication()
        {

            var collectionUri = new Uri("https://dev.azure.com/LATeamInc/");
            var credential = new NetworkCredential("username", "password");
            teamProjectCollection = new TfsTeamProjectCollection(collectionUri, credential);
            try
            {
               teamProjectCollection.EnsureAuthenticated();
            }
            catch (Exception)
            {
                return null;
            }
            return teamProjectCollection;
            
        }

        public bool IsAuthenticate() => teamProjectCollection.HasAuthenticated;
    }

}