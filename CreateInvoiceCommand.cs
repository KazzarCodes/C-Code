using iSync.Api;
using iSync.SyncApi.Models;
using iSync.SyncApi.Models.CommandModels;
using iSync.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSync.SyncApi.Commands.CreateInvoice
{
    public class CreateInvoiceCommand : CommandBase, ICommand<CreateInvoiceCommandModel>
    {
        private CreateInvoiceCommandModel _commandModel;
        private Dto.Customer _customer;

        public CreateInvoiceCommand(IDynamicRepository repository)
            :base(repository)
        {
        }

        public void Invoke(CreateInvoiceCommandModel commandModel)
        {
            // Initialize
            _commandModel = commandModel;

            if (_commandModel.InvoiceId == 0)
            {
                // Invoice header
                var invHeader = GetInvHeaderEntity();

                // Invoice items
                foreach (var invItemModel in commandModel.InvoiceItems)
                {
                    var invItem = GetInvItemEntity(invItemModel);
                    invHeader.ProductInvoiceDetails.Add(invItem);
                }

                // Create Invoice
                _repository.Persist();
                commandModel.Result = invHeader.Id.Value;

                // Post update logic for stock (handled in stored proc)
                var invoiceIDsParam = SqlClientUtil.CreateIntListParameter("@ProductInvoiceIDs", invHeader.Id.Value);
                _repository.ExecuteStoredProcedure("[API].[spCompleteInvoices]", invoiceIDsParam);
            }
            else
            {
                // Updating Term 
                var invoice = _repository.GetQuery<Dto.ProductInvoiceHeader>(s => s.Id == _commandModel.InvoiceId).FirstOrDefault();

                invoice.Comment = _commandModel.Comment;
                invoice.TermId = _commandModel.TermId;

                _repository.Persist();
            }
        }

        private Dto.ProductInvoiceHeader GetInvHeaderEntity()
        {
            Dto.ProductInvoiceHeader invoice = new Dto.ProductInvoiceHeader
            {
                IsCreditNote = false,
                TaxTypeId = _commandModel.TaxTypeId,
                SubCustomerId = _commandModel.SubCustomerId,
                SalesOrderId = _commandModel.SalesOrderId,
                InvoicedUserId = _commandModel.UserId,
                InvoicedDate = _commandModel.InvoiceDate,
                Reference = _commandModel.Reference,
                Comment = _commandModel.Comment,
                LinkedJobId = _commandModel.LinkedJobId,
                DeliveryAddressDetailId = addressDetailId,
                TermId = _commandModel.TermId,
            };

            _repository.Add(invoice);

            return invoice;
        }

        private Dto.ProductInvoiceDetail GetInvItemEntity(InvoiceItemModel invItemModel)
        {
            Dto.ProductInvoiceDetail invoiceItem = new Dto.ProductInvoiceDetail();

            // Create invoice item entity
            invItemModel.Reference = invItemModel.Reference ?? "";
            invoiceItem.SoDetailId = invItemModel.SoDetailId;
            invoiceItem.ProductStockMoveHeaderId = invItemModel.PickSlipId;
            invoiceItem.Commission = invItemModel.Commission;
            invoiceItem.Discount = invItemModel.Discount;
            invoiceItem.SellingPrice = invItemModel.SellingPrice;
            invoiceItem.CommissionId = invItemModel.CommissionId;
            invoiceItem.Units = invItemModel.Units;
            invoiceItem.CnTypeId = invItemModel.CnTypeId;
            invoiceItem.Comment = invItemModel.Comment;
            invoiceItem.JobId = invItemModel.JobId;
            invoiceItem.CurrencyId = invItemModel.CurrencyId;
            invoiceItem.ExRate = invItemModel.ExRate;
            invoiceItem.Rate = invItemModel.Rate;
            invoiceItem.RepCodeId = invItemModel.RepCodeId;

            return invoiceItem;
        }
    }
}
