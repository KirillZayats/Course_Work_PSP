using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixLibrary
{
    class GaussSolutionNotFound : Exception
    {
        public GaussSolutionNotFound(string msg) : base("Решение не может быть найдено: \r\n" + msg){ }
    }
}
