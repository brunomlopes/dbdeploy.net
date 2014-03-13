using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database.Reader
{
    class DefaultParameterReaderTest
    {
        private IParameterReader parameterReader;
        private Mock<IDataReader> dataReader;

        [Test]
        public void ShouldGetCorrectShortValue()
        {
            const short value = 3;

            dataReader = new Mock<IDataReader>();
            dataReader.SetupGet(dr => dr[It.IsAny<string>()]).Returns(value);

            parameterReader = new DefaultParameterReader();
            var obtainedValue = parameterReader.GetShort(dataReader.Object, "field_name");

            obtainedValue.Should().Be(value);
        }

        [Test]
        public void ShouldGetCorrectIntValue()
        {
            const byte value = 3;

            dataReader = new Mock<IDataReader>();
            dataReader.SetupGet(dr => dr[It.IsAny<string>()]).Returns(value);

            parameterReader = new DefaultParameterReader();
            var obtainedValue = parameterReader.GetByte(dataReader.Object, "field_name");

            obtainedValue.Should().Be(value);
        }

        [Test]
        public void ShouldGetCorrectStringValue()
        {
            const string value = "anythingValue";

            dataReader = new Mock<IDataReader>();
            dataReader.SetupGet(dr => dr[It.IsAny<string>()]).Returns(value);

            parameterReader = new DefaultParameterReader();
            var obtainedValue = parameterReader.GetString(dataReader.Object, "field_name");

            obtainedValue.Should().Be(value);
        }
    }
}