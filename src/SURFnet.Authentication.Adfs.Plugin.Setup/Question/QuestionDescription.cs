﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class QuestionDescription
    {
        public string Introduction { get; set; }
        public string Question { get; set; }
        public string[] Options { get; set; }
    }
}
