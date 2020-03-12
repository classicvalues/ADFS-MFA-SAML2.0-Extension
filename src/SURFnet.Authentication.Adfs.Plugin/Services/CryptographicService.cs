﻿/*
* Copyright 2017 SURFnet bv, The Netherlands
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using log4net;

    using SURFnet.Authentication.Adfs.Plugin.Common.Exceptions;
    using SURFnet.Authentication.Adfs.Plugin.Common.Services.Interfaces;
    using SURFnet.Authentication.Adfs.Plugin.Configuration;
    using SURFnet.Authentication.Adfs.Plugin.Models;

    using Sustainsys.Saml2;

    /// <summary>
    /// Handles the signing.
    /// </summary>
    public sealed class CryptographicService : IDisposable
    {
        /// <summary>
        /// Gets the signing algorithm for the SAML request.
        /// </summary>
        /// <value>The signing algorithm.</value>
        private const string SigningAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        /// <summary>
        /// To enable SHA256 signatures in the SAML Library we need to enable this only once.
        /// </summary>
        private static bool isSha265Enabled;

        /// <summary>
        /// Used for logging.
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Contains the signing certificate.
        /// </summary>
        private X509Certificate2 signingCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptographicService" /> class.
        /// </summary>
        /// <param name="certificateService">The certificate service.</param>
        public CryptographicService(ICertificateService certificateService)
        {
            EnableSha256Support();
            this.log = LogManager.GetLogger("CryptographicService");
            this.LoadCertificate(certificateService);
        }

        /// <summary>
        /// Signs the SAML request.
        /// </summary>
        /// <param name="authRequest">The authentication request.</param>
        /// <returns>
        /// The signed XML.
        /// </returns>
        public string SignSamlRequest(Saml2AuthenticationSecondFactorRequest authRequest)
        {
            var xmlDoc = XmlHelpers.XmlDocumentFromString(authRequest.ToXml());
            xmlDoc.Sign(this.signingCertificate, true, SigningAlgorithm);
            var xml = xmlDoc.OuterXml;
            var encodedXml = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));
            return encodedXml;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.signingCertificate.Dispose();
            this.signingCertificate = null;
        }

        /// <summary>
        /// To enable SHA256 signatures in the SAML Library we need to enable this only once.
        /// </summary>
        private static void EnableSha256Support()
        {
            if (isSha265Enabled)
            {
                return;
            }

            /// TODO: Doublecheck if indeed now default if not specified in config (Plem)
            //Sustainsys.Saml2.Configuration.Options.GlobalEnableSha256XmlSignatures();
            isSha265Enabled = true;
        }

        /// <summary>
        /// Loads the certificate.
        /// </summary>
        /// <param name="certificateService">The certificate service.</param>
        private void LoadCertificate(ICertificateService certificateService)
        {
            try
            {
                this.signingCertificate = certificateService.GetCertificate(StepUpConfig.Current.LocalSpConfig.SPSigningCertificate);
            }
            catch (Exception e)
            {
                this.log.FatalFormat("Error while loading signing certificate. Details: {0}", e);
                throw new InvalidConfigurationException("ERROR_0002", "Error while loading signing certificate", e);
            }
        }
    }
}
