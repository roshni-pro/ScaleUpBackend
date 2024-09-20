using MassTransit;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.DSA;
using ScaleUP.BuildingBlocks.EventBus.Messages.LeadActivity.KYCActivity;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Global.Infrastructure.Persistence.Interfaces;
using ScaleUP.Services.LocationAPI.Managers;
using ScaleUP.Services.LocationAPI.Persistence;
using ScaleUP.Services.LocationModels.Master;

namespace ScaleUP.Services.LocationAPI.Consumers
{
    public class UpdatingAddressEventConsumer : IConsumer<IUpdatingAddressEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly LocationManager _locationManager;
        //private readonly ILogger<KYCSuccessEventConsumer> _logger;
        private readonly IMassTransitService _massTransitService;
        public UpdatingAddressEventConsumer(ApplicationDbContext context, IMassTransitService massTransitService, LocationManager locationManager)
        {
            _context = context;
            //_logger = logger;
            _massTransitService = massTransitService;
            _locationManager = locationManager;
        }

        public async Task Consume(ConsumeContext<IUpdatingAddressEvent> context)
        {

            if (context.Message.KYCMasterCode == KYCMasterConstants.PersonalDetail)
            {
                var KYCPersonalDetailActivity = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCPersonalDetailActivity>(context.Message.JSONString);

                //LocationManager locationManager = new LocationManager(_context);

                List<LocationOnSave> locations = new List<LocationOnSave>();
                locations.Add(new LocationOnSave
                {
                    AddressLineOne = KYCPersonalDetailActivity.PermanentAddressLine1,
                    ZipCode = int.Parse(KYCPersonalDetailActivity.PermanentPincode),
                    AddressLineTwo = KYCPersonalDetailActivity.PermanentAddressLine2,
                    AddressLineThree = "",
                    CityId = int.Parse(KYCPersonalDetailActivity.PermanentCity),
                    AddressType = AddressTypeConstants.Permanent
                });

                locations.Add(new LocationOnSave
                {
                    AddressLineOne = KYCPersonalDetailActivity.ResAddress1,
                    ZipCode = int.Parse(KYCPersonalDetailActivity.Pincode),
                    AddressLineTwo = KYCPersonalDetailActivity.ResAddress2,
                    AddressLineThree = "",
                    CityId = int.Parse(KYCPersonalDetailActivity.City),
                    AddressType = AddressTypeConstants.Current
                });

                var response = _locationManager.SaveLocation(locations);

                KYCPersonalDetailActivity.CurentLocationId = response.Where(x => x.AddressType == AddressTypeConstants.Current).First().Id;
                KYCPersonalDetailActivity.PermanentLocationId = response.Where(x => x.AddressType == AddressTypeConstants.Permanent).First().Id;

                var leadActivityCreatedEvent = new LeadActivityCreatedEvent
                {
                    CorrelationId = context.Message.CorrelationId,
                    JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(KYCPersonalDetailActivity),
                    KYCMasterCode = context.Message.KYCMasterCode,
                    UserId = context.Message.UserId,
                    ActivityId = context.Message.ActivityId,
                    LeadId = context.Message.LeadId,
                    SubActivityId = context.Message.SubActivityId,
                    ComapanyId = context.Message.ComapanyId,
                    ProductCode = context.Message.ProductCode
                };

                await _massTransitService.Publish(leadActivityCreatedEvent);
            }
            else if (context.Message.KYCMasterCode == KYCMasterConstants.BuisnessDetail)
            {
                var KYCBusinessActivityDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<BusinessActivityDetail>(context.Message.JSONString);

                //LocationManager locationManager = new LocationManager(_context);

                List<LocationOnSave> locations = new List<LocationOnSave>();
                locations.Add(new LocationOnSave
                {
                    AddressLineOne = KYCBusinessActivityDetail.BusAddCorrLine1,
                    ZipCode = int.Parse(KYCBusinessActivityDetail.BusAddCorrPincode),
                    AddressLineTwo = KYCBusinessActivityDetail.BusAddCorrLine2,
                    AddressLineThree = "",
                    CityId = int.Parse(KYCBusinessActivityDetail.BusAddCorrCity),
                    AddressType = AddressTypeConstants.Current
                });

                //locations.Add(new LocationOnSave
                //{
                //    AddressLineOne = KYCBusinessActivityDetail.BusAddPerLine1,
                //    ZipCode = int.Parse(KYCBusinessActivityDetail.BusAddPerPincode),
                //    AddressLineTwo = KYCBusinessActivityDetail.BusAddPerLine2,
                //    AddressLineThree = "",
                //    CityId = int.Parse(KYCBusinessActivityDetail.BusAddPerCity),
                //    AddressType = AddressTypeConstants.Permanent
                //});

                var response = _locationManager.SaveLocation(locations);

                KYCBusinessActivityDetail.CurentLocationId = response.Where(x => x.AddressType == AddressTypeConstants.Current).First().Id;
                //KYCBusinessActivityDetail.PermanentLocationId = response.Where(x => x.AddressType == AddressTypeConstants.Permanent).First().Id;

                var leadActivityCreatedEvent = new LeadActivityCreatedEvent
                {
                    CorrelationId = context.Message.CorrelationId,
                    JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(KYCBusinessActivityDetail),
                    KYCMasterCode = context.Message.KYCMasterCode,
                    UserId = context.Message.UserId,
                    ActivityId = context.Message.ActivityId,
                    LeadId = context.Message.LeadId,
                    SubActivityId = context.Message.SubActivityId,
                    ComapanyId = context.Message.ComapanyId,
                    ProductCode = context.Message.ProductCode
                };

                await _massTransitService.Publish(leadActivityCreatedEvent);
            }
            else if (context.Message.KYCMasterCode == KYCMasterConstants.Aadhar)
            {
                var KarzaAadharDocType = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCActivityAadhar>(context.Message.JSONString);
                //LocationManager locationManager = new LocationManager(_context);

                LocationRawOnSave locations = null;
                if (KarzaAadharDocType.aadharInfo != null && KarzaAadharDocType.aadharInfo.address != null && KarzaAadharDocType.aadharInfo.address.splitAddress != null)
                {

                    string addLineOne = "";
                    string addLineTwo = "";
                    string addLineThree = "";
                    string city = "";
                    string state = "";
                    string zip = "";
                    string country = "";
                    string houseNumber = "";

                    if (!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.HouseNumber))
                    {
                        addLineOne = KarzaAadharDocType.aadharInfo.address.splitAddress.HouseNumber;

                    }
                    if (!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.Street))
                    {
                        addLineOne = (addLineOne == "" ? "" : addLineOne + ", ") + KarzaAadharDocType.aadharInfo.address.splitAddress.Street;

                    }
                    if ((!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.HouseNumber)) && (!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.Street)))
                    {   
                        houseNumber = KarzaAadharDocType.aadharInfo.address.splitAddress.HouseNumber;
                        addLineOne = (houseNumber == "" ? "" : houseNumber + ", ") + KarzaAadharDocType.aadharInfo.address.splitAddress.Street;
                    }

                    if (!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.Landmark))
                    {
                        addLineTwo = KarzaAadharDocType.aadharInfo.address.splitAddress.Landmark;

                    }
                    if (!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.Location))
                    {
                        addLineTwo = (addLineTwo == "" ? "" : addLineTwo+ ", ") + KarzaAadharDocType.aadharInfo.address.splitAddress.Location;

                    }

                    if (!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.PostOffice))
                    {
                        addLineThree = KarzaAadharDocType.aadharInfo.address.splitAddress.PostOffice;

                    }



                    if (!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.VtcName))
                    {
                        city = KarzaAadharDocType.aadharInfo.address.splitAddress.VtcName;
                    }
                    else if (!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.Subdistrict))
                    {
                        city = KarzaAadharDocType.aadharInfo.address.splitAddress.Subdistrict;
                    }
                    else if (!string.IsNullOrEmpty(KarzaAadharDocType.aadharInfo.address.splitAddress.District))
                    {
                        city = KarzaAadharDocType.aadharInfo.address.splitAddress.District;
                    }

                    state = KarzaAadharDocType.aadharInfo.address.splitAddress.State;

                    zip = KarzaAadharDocType.aadharInfo.address.splitAddress.Pincode;

                    country = KarzaAadharDocType.aadharInfo.address.splitAddress.Country;


                    if (string.IsNullOrEmpty(addLineOne) && !string.IsNullOrEmpty(addLineTwo))
                    {
                        addLineOne = addLineTwo;
                        addLineTwo = addLineThree;
                        addLineThree = "";
                    }

                    if (string.IsNullOrEmpty(addLineOne) && string.IsNullOrEmpty(addLineTwo) && !string.IsNullOrEmpty(addLineThree))
                    {
                        addLineOne = addLineThree;
                        addLineThree = "";
                    }

                    locations = new LocationRawOnSave
                    {
                        AddressLineOne = addLineOne,
                        AddressLineTwo = addLineTwo,
                        AddressLineThree = addLineThree,
                        ZipCode = int.Parse(zip),
                        City = city,
                        Country = country,
                        AddressType = AddressTypeConstants.Current,
                        State = state
                    };

                    var response = _locationManager.SaveRawLocation(locations);

                    if (response != null && response.Id > 0)
                    {
                        KarzaAadharDocType.aadharAddressId = response.Id;


                        var leadActivityCreatedEvent = new LeadActivityCreatedEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(KarzaAadharDocType),
                            KYCMasterCode = context.Message.KYCMasterCode,
                            UserId = context.Message.UserId,
                            ActivityId = context.Message.ActivityId,
                            LeadId = context.Message.LeadId,
                            SubActivityId = context.Message.SubActivityId,
                            ComapanyId = context.Message.ComapanyId,
                            ProductCode = context.Message.ProductCode
                        };

                        await _massTransitService.Publish(leadActivityCreatedEvent);
                    }
                    else
                    {
                        var kycFailEvent = new KYCFailEvent
                        {
                            CorrelationId = context.Message.CorrelationId,
                            ActivityId = context.Message.ActivityId,
                            LeadId = context.Message.LeadId,
                            SubActivityId = context.Message.SubActivityId,
                            ErrorMessage = "Aadhar address not updated",
                        };

                        await _massTransitService.Publish(kycFailEvent);
                    }
                }



            }
            else if (context.Message.KYCMasterCode == KYCMasterConstants.ConnectorPersonalDetail)
            {
                var KYCConnectorPersonalDetailActivity = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCConnectorPersonalDetailActivity>(context.Message.JSONString);

                //LocationManager locationManager = new LocationManager(_context);

                List<LocationOnSave> locations = new List<LocationOnSave>();
                locations.Add(new LocationOnSave
                {
                    AddressLineOne = KYCConnectorPersonalDetailActivity.Address,
                    ZipCode = int.Parse(KYCConnectorPersonalDetailActivity.Pincode),
                    AddressLineTwo = "",
                    AddressLineThree = "",
                    CityId = int.Parse(KYCConnectorPersonalDetailActivity.City),
                    AddressType = AddressTypeConstants.Permanent
                });

                //locations.Add(new LocationOnSave
                //{
                //    AddressLineOne = KYCPersonalDetailActivity.ResAddress1,
                //    ZipCode = int.Parse(KYCPersonalDetailActivity.Pincode),
                //    AddressLineTwo = KYCPersonalDetailActivity.ResAddress2,
                //    AddressLineThree = "",
                //    CityId = int.Parse(KYCPersonalDetailActivity.City),
                //    AddressType = AddressTypeConstants.Current
                //});

                var response = _locationManager.SaveLocation(locations);

                //KYCConnectorPersonalDetailActivity.CurentLocationId = response.Where(x => x.AddressType == AddressTypeConstants.Current).First().Id;
                KYCConnectorPersonalDetailActivity.PermanentLocationId = response.Where(x => x.AddressType == AddressTypeConstants.Permanent).First().Id;

                var leadActivityCreatedEvent = new LeadActivityCreatedEvent
                {
                    CorrelationId = context.Message.CorrelationId,
                    JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(KYCConnectorPersonalDetailActivity),
                    KYCMasterCode = context.Message.KYCMasterCode,
                    UserId = context.Message.UserId,
                    ActivityId = context.Message.ActivityId,
                    LeadId = context.Message.LeadId,
                    SubActivityId = context.Message.SubActivityId,
                    ComapanyId = context.Message.ComapanyId,
                    ProductCode = context.Message.ProductCode
                };

                await _massTransitService.Publish(leadActivityCreatedEvent);
            }
            else if (context.Message.KYCMasterCode == KYCMasterConstants.DSAPersonalDetail)
            {
                var KYCDSAPersonalDetailActivity = Newtonsoft.Json.JsonConvert.DeserializeObject<KYCDSAPersonalDetailActivity>(context.Message.JSONString);

                //LocationManager locationManager = new LocationManager(_context);

                List<LocationOnSave> locations = new List<LocationOnSave>();
                locations.Add(new LocationOnSave
                {
                    AddressLineOne = KYCDSAPersonalDetailActivity.Address,
                    ZipCode = int.Parse(KYCDSAPersonalDetailActivity.Pincode),
                    AddressLineTwo = "",
                    AddressLineThree = "",
                    CityId = int.Parse(KYCDSAPersonalDetailActivity.City),
                    AddressType = AddressTypeConstants.Permanent
                });

                locations.Add(new LocationOnSave
                {
                    AddressLineOne = KYCDSAPersonalDetailActivity.CompanyAddress,
                    ZipCode = int.Parse(KYCDSAPersonalDetailActivity.CompanyPincode),
                    AddressLineTwo = "",
                    AddressLineThree = "",
                    CityId = int.Parse(KYCDSAPersonalDetailActivity.CompanyCity),
                    AddressType = AddressTypeConstants.Current
                });

                var response = _locationManager.SaveLocation(locations);

                KYCDSAPersonalDetailActivity.CurentLocationId = response.Where(x => x.AddressType == AddressTypeConstants.Current).First().Id;
                KYCDSAPersonalDetailActivity.PermanentLocationId = response.Where(x => x.AddressType == AddressTypeConstants.Permanent).First().Id;

                var leadActivityCreatedEvent = new LeadActivityCreatedEvent
                {
                    CorrelationId = context.Message.CorrelationId,
                    JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(KYCDSAPersonalDetailActivity),
                    KYCMasterCode = context.Message.KYCMasterCode,
                    UserId = context.Message.UserId,
                    ActivityId = context.Message.ActivityId,
                    LeadId = context.Message.LeadId,
                    SubActivityId = context.Message.SubActivityId,
                    ComapanyId = context.Message.ComapanyId,
                    ProductCode = context.Message.ProductCode
                };

                await _massTransitService.Publish(leadActivityCreatedEvent);
            }




        }
    }
}
