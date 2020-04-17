﻿using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// As regular, IdP Metadata in ADFS directory. Copies it on Install().
    /// </summary>
    public class Sustainsys2_3MdComponent : Sustainsys2_xComponent
    {
        public Sustainsys2_3MdComponent() : base("Sustainsys.Saml2 v2.3 from Metadata")
        {
            ConfigParameters = new string[]
            {
                ConfigSettings.IdPEntityId,
                ConfigSettings.IdPMdFilename,
                ConfigSettings.SPEntityId,
                ConfigSettings.SPSignThumb1
            };
        }

        public override int Install()
        {
            int rc = base.Install(); // first regular install

            if ( rc == 0 )
            {
                // now metadata from "config" to ADFS directory
                string filename = ConfigSettings.IdPMetadataFilename.Value;
                rc = FileService.FileCopy(FileDirectory.Config, FileDirectory.AdfsDir, filename);
            }

            return rc;
        }

        protected override List<Setting> ExctractSustainsysConfig()
        {
            List<Setting> settings = new List<Setting>();

            string sustainsysCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.SustainCfgFilename);
            var sustainsysConfig = XDocument.Load(sustainsysCfgPath);

            var sustainsysSection = sustainsysConfig.Descendants(XName.Get(SustainsysSaml2Section)).FirstOrDefault();

            // SP entityID
            ConfigSettings.SPEntityID.FoundCfgValue = sustainsysSection?.Attribute(XName.Get(EntityId))?.Value;
            settings.Add(ConfigSettings.SPEntityID);

            // First "serviceCertificates" element, then first the "add" certificate element and its "findValue" attribute
            var spcerts = sustainsysSection?.Descendants(SPCerts).FirstOrDefault();
            var firstcert = spcerts?.Descendants(XName.Get("add")).FirstOrDefault();
            ConfigSettings.SPPrimarySigningThumbprint.FoundCfgValue = firstcert?.Attribute(XName.Get(CertFindValue))?.Value;
            settings.Add(ConfigSettings.SPPrimarySigningThumbprint);

            // get the first IdP from the list
            var identityProviders = sustainsysSection?.Descendants(SustainIdentityProviders).FirstOrDefault();
            var identityProvider = identityProviders?.Descendants(XName.Get("add")).FirstOrDefault();

            // IdP entityID attribute
            ConfigSettings.IdPEntityID.FoundCfgValue = identityProvider?.Attribute(XName.Get(EntityId))?.Value;
            settings.Add(ConfigSettings.IdPEntityID);

            // metadataLocation attribute
            ConfigSettings.IdPMetadataFilename.FoundCfgValue = identityProvider?.Attribute(XName.Get(MdLocationAttribute))?.Value;
            settings.Add(ConfigSettings.IdPMetadataFilename);

            return settings;
        }
    }
}
