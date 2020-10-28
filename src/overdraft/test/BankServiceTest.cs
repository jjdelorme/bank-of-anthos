using System;
using Anthos.Samples.BankOfAnthos.Overdraft;
using Xunit;

namespace Anthos.Samples.BankOfAnthos.Overdraft.Tests
{
    public class BankServiceTest
    {
        [Fact]
        public void GetBalance()
        {
            string accountNum = "1011226111";
            string bearer = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJ1c2VyIjoidGVzdHVzZXIiLCJhY2N0IjoiMTAxMTIyNjExMSIsIm5hbWUiOiJUZXN0IFVzZXIiLCJpYXQiOjE2MDQwNjQ0MjcsImV4cCI6MTYwNDA2ODAyN30.dqgxiY5bYElczeqRfBm3Eu_uxMliWiVKmLA6FrO2WeNvqxHvepeiOz4XHjMh6AaXrAoUr2Bpk6_gBtimkiubYR4DPMNyFp8V0QoYoUqtoL81kOdS0FmR0Bhg8JFfUQ4QMyfkIcrZLanyulkc7glWIe10b1FujlAbQWh7avVzMb4Le7dLDQpaforoSuJQbRpFGYFJA65FFg_vDBou4CNINy-MjiyW6EVm4pd_qcDowDTsVrN8H-jL3GWUgZOeHcgjVNsPEiD_0BgEiv896jv9Qfz9g8hpyEfBLNaCH9IbKL98vk0Q03FWOumbqFkWYtuwUrKQoQ3wX99W48ls8p7syfDTqI3qU0MuAAO0fOPAQkQ2FMRqdfVVMLeVgOrzYVovpgLO8Ka_zqA_g-_dOXyl-N5NiOpw-oc6fhmGZkmxEk-V_apk4tuKUOcp1X1iPv4WttAu0yoIDflBkSpwll5THLr17osXgujUsKBuZgVj4yHjzIliPnYaTEmV__29bZLIMbjH0vyj4tRBMhhvj5CFDgIPDi7foaW_HDFJkgykyqXy0JZXma3mmU5UtVN0mljhDEwh4Euq4vPX0FfkHMUQg_vFVXJb2v0dmEEq5482-wJgCsScaJPYCpAF2PTW9jUBMzbRJWSJ2xl87x2pUONnUl9LJTM5uK0dRzMUBRg2XfI";
            BankService service = new BankService(null); // <-- need to mock configuration here
            long balance = service.GetBalance(bearer, accountNum);

            Assert.Equal(142695, balance);
        }
    }
}