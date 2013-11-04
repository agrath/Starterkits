using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Overflow.Events
{
    public class AddPortfolioCropperToImageMediaType
    {
        public static void Execute()
        {
            const string propertyGroupName = "Crop";
            const string propertyAlias = "umbCrop";
            
            var contentTypeService = UmbracoContext.Current.Application.Services.ContentTypeService;
            var imageMediaType = contentTypeService.GetAllMediaTypes().Single(x => x.Alias == Constants.Conventions.MediaTypes.Image);

            if (imageMediaType.PropertyGroups.Any(x => x.Name == propertyGroupName) == false)
                imageMediaType.AddPropertyGroup(propertyGroupName);

            contentTypeService.Save(imageMediaType);

            if (imageMediaType.PropertyTypes.Any(x => x.Alias == propertyAlias)) 
                return;

            var dataTypeService = UmbracoContext.Current.Application.Services.DataTypeService;
            var portfolioPickerDataTypeDefinition = dataTypeService.GetAllDataTypeDefinitions().Single(x => x.Name == "PortfolioCropper");
            var portfolioPickerPropertyType = new PropertyType(portfolioPickerDataTypeDefinition) { Alias = propertyAlias, Name = propertyGroupName };

            imageMediaType.AddPropertyType(portfolioPickerPropertyType, propertyGroupName);

            contentTypeService.Save(imageMediaType);
        }
    }
}