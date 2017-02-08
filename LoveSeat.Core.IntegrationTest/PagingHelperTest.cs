using LoveSeat.Core;
using LoveSeat.Core.Interfaces;
using Moq;
using Xunit;

namespace LoveSeat.IntegrationTest
{
    public class PagingHelperTest
    {
        [Fact]
        public void Should_Show_Previous_If_OffSet_Not_Equal_Zero()
        {
            var result = new Mock<IViewResult>();
            result.SetupGet(x => x.OffSet).Returns(1);
            var options = new ViewOptions();
            var model = new PageableModel();
            model.UpdatePaging(options, result.Object);
            Assert.True(model.ShowPrev);        
        }

        [Fact]
        public void Should_Not_Show_Previous_If_Offset_Equal_Zero()
        {
            var result = new Mock<IViewResult>();
            result.SetupGet(x => x.OffSet).Returns(0);
            var options = new ViewOptions();
            var model = new PageableModel();
            model.UpdatePaging(options, result.Object);
            Assert.False(model.ShowPrev);        
        }

        [Fact]
        public void Should_Show_Next_Unless_Offset_Plus_Limit_Gte_Count()
        {
            var result = new Mock<IViewResult>();
            result.SetupGet(x => x.OffSet).Returns(5);
            var options = new ViewOptions();
            options.Limit = 5;
            result.SetupGet(x => x.TotalRows).Returns(10);
            var model = new PageableModel();
            model.UpdatePaging(options, result.Object);
            Assert.False(model.ShowNext);
        }
    }
}