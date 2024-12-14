using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using FortitudeAsia.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FortitudeAsia.Controllers
{
    [Route("api/submittrxmessage")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TransactionController));

        // POST transaction (To receive transaction from partner for further processing)
        [HttpPost]
        public ActionResult Post([FromBody] TransactionRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            _logger.Info($"Request => {JsonSerializer.Serialize(request)}");

            var partner = PartnerDb.GetPartners().FirstOrDefault(p => p.AllowedPartner == request.partnerkey);
            
            long totalamount = request.totalamount;
            long totaldiscount = CalculateTotalDiscount(totalamount);
            long finalamount = totalamount - totaldiscount;
            bool isFailValidation = false;
            var signature = SHA256Encoding(request.timestamp.ToString("yyyyMMddHHmmss") + request.partnerkey + request.partnerrefno + request.totalamount + request.partnerpassword);

            Response response = new()
            {
                result = 1,
                totalamount = totalamount,
                totaldiscount = totaldiscount,
                finalamount = finalamount,
                resultmessage = null
            };

            // Validate is authorised parthner with correct credentials and signature 
            if (partner == null || partner.PartnerNo != request.partnerrefno || partner.Password != Base64Decode(request.partnerpassword) || signature != request.sig)
            {
                response = new()
                {
                    result = 0,
                    totalamount = null,
                    totaldiscount = null,
                    finalamount = null,
                    resultmessage = "Access denied!"
                };

                isFailValidation = true;
            }

            // Validate is correct total amount in itemDetails
            long totalSum = 0;
            if (request.items != null)
            {                
                foreach (var item in request.items)
                {
                    totalSum += (long)(item.qty * item.unitprice);
                }

                if (totalSum != totalamount)
                {
                    response = new()
                    {
                        result = 0,
                        totalamount = null,
                        totaldiscount = null,
                        finalamount = null,
                        resultmessage = "Invalid Total Amount."
                    };

                    isFailValidation = true;
                }
            }
            
            // Validate if provided timestamp exceed server time
            if (!IsValidTimestamp(request.timestamp))
            {
                response = new()
                {
                    result = 0,
                    resultmessage = "Expired."
                };

                isFailValidation = true;
            }
            if (isFailValidation)
            {
                _logger.Error($"Response => {JsonSerializer.Serialize(response)}");
                return Ok(response);
            }
                        
            _logger.Info($"Response => {JsonSerializer.Serialize(response)}");
            return Ok(response);

        }

        private static string? Base64Decode(string encoded)
        {
            try
            {               
                return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            }
            catch (Exception ex)
            {
                // If provided string to decode does not match the criteria/fails to decode, return null
                return null;
            }
        }

        private static string SHA256Encoding(string message)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(message.ToString()));

                // Convert the binary hash to a hexadecimal string
                StringBuilder hexBuilder = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    hexBuilder.AppendFormat("{0:x2}", b); // Convert each byte to a two-character hex string
                }

                // Encode the hexadecimal string into Base64
                string hexString = hexBuilder.ToString();
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(hexString));
            }
        }

        public static bool IsValidTimestamp(DateTime givenTimestamp)
        {
            // Get the current server time (UTC)
            DateTime serverTime = DateTime.UtcNow;

            // Define the allowed time window (+-5 minutes)
            TimeSpan allowableTimeWindow = TimeSpan.FromMinutes(5);

            // Check if the given timestamp is within the allowed time window of the server time
            return givenTimestamp >= serverTime - allowableTimeWindow && givenTimestamp <= serverTime + allowableTimeWindow;
        }

        private static long CalculateTotalDiscount(long totalAmount)
        {
            decimal totalDiscount = 0;

            // Base Discounts
            if (ConvertFromCent(totalAmount) < 200) // 0%
            {
                totalDiscount = 0;
            }
            else if (ConvertFromCent(totalAmount) >= 200 && ConvertFromCent(totalAmount) <= 500) // 5% discount
            {
                totalDiscount = 0.05m;
            }
            else if (ConvertFromCent(totalAmount) >= 501 && ConvertFromCent(totalAmount) <= 800) // 7% discount
            {
                totalDiscount = 0.07m;
            }
            else if (ConvertFromCent(totalAmount) >= 801 && ConvertFromCent(totalAmount) <= 1200) // 10% discount
            {
                totalDiscount = 0.10m;
            }
            else if (ConvertFromCent(totalAmount) > 1200)
            {
                totalDiscount = 0.15m;
            }

            // Conditional Discounts
            if (ConvertFromCent(totalAmount) > 500 && IsPrime(ConvertFromCent(totalAmount))) // is prime number, +8% discount
            {
                totalDiscount += 0.08m;
            }
            else if (ConvertFromCent(totalAmount) > 900 && ConvertFromCent(totalAmount) % 10 == 5) // ends in digit 5, +10% discount
            {
                totalDiscount += 0.10m;
            }

            // Apply cap on maximum discount
            if (totalDiscount > 0.20m)
            {
                totalDiscount = 0.20m;
            }

            return (long)(totalAmount * totalDiscount);
        }

        private static long ConvertFromCent(long total)
        {
            return total/100;
        }

        private static bool IsPrime(decimal number)
        {
            if (number < 2)
            {
                return false;
            }

            for (int i = 2; i <= Math.Sqrt((double)number); i++)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

}

