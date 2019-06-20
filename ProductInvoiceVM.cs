public class ProductInvoiceVM : PersistableViewModelBase
{
    protected override ICommandModel GetCommandModel()
    {
        var lineItems = new List<InvoiceItemModel>();

        foreach (var item in ItemModels.Where(im => im.IncludeInInvoice))
        {
            var mappedItem = AutoMapper.MapObject(item, new InvoiceItemModel());
            mappedItem.Units = (decimal)item.PickedUnits;
            lineItems.Add(mappedItem);
        }

        var commandModel = new CreateInvoiceCommandModel()
        {
            SalesOrderId = SalesOrderId,
            PickTicketId = PickTicketId,
            CustomerId = CustomerId,
            SubCustomerId = SubCustomerId,
            InvoiceDate = InvoicedDate,
            Comment = Comment,
            Reference = Reference,
            AddressDetailId = DeliveryAddressDetailId,
            LinkedJobId = 0,
            TaxTypeId = TaxTypeId,
            TermId = TermId,
            InvoiceItems = lineItems
        };

        return commandModel;
    }
}
