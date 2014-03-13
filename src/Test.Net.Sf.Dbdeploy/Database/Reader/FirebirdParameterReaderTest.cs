using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database.Reader
{
    class FirebirdParameterReaderTest
    {
        private IParameterReader parameterReader;
        private Mock<IDataReader> dataReader;

        [Test]
        public void ShouldGetCorrectStringValue()
        {
            var value = new byte[] {65, 110, 121};

            dataReader = new Mock<IDataReader>();
            dataReader.SetupGet(dr => dr[It.IsAny<string>()]).Returns(value);

            parameterReader = new FirebirdParameterReader();
            var obtainedValue = parameterReader.GetLongValueString(dataReader.Object, "field_name");

            obtainedValue.Should().Be("Any");
        }
    }
}
