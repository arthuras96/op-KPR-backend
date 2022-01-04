using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Functions
{
    public class Email
    {
        public void EmailSender(string from, string recepient, string cc,
                               string bcc, string subject, string body)
        {
            try
            {
                //instancio a classe MailMessage, responsável por atribuir
                //os valores para as variáveis declaradas no método
                MailMessage mail = new MailMessage();

                //instancio o SMTP passando o host configurado no IIS
                //SmtpClient smtp = new SmtpClient("smtplw.com.br", 587);
                SmtpClient smtp = new SmtpClient("email-ssl.com.br", 587);

                //endereço do remetente, chamo o método From que recebe uma nova
                //instância de MailAdress passando como parâmetro a variável from
                if (string.IsNullOrEmpty(from))
                    mail.From = new MailAddress("noreply@xpsat.com.br");
                else
                    mail.From = new MailAddress(from);

                //destinatário, uso método Add, já que posso enviar para várias pessoas
                mail.To.Add(new MailAddress(recepient));

                //faço uma verificação, se houver cc e bcc, envio
                if (cc != "")
                {
                    mail.CC.Add(new MailAddress(cc));
                }

                if (bcc != "")
                {
                    mail.Bcc.Add(new MailAddress(bcc));
                }

                //defino o assunto
                mail.Subject = subject;

                //defino o corpo da mensagem
                mail.Body = body;

                //defino que o formato do texto será HTML
                mail.IsBodyHtml = true;

                //minha prioridade de envio será normal
                mail.Priority = MailPriority.Normal;

                //smtp.UseDefaultCredentials = false;
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential("noreply@xpsat.com.br", "SAT3$2020@n");


                //envio o mail por meio do método Send, passando como
                //parâmetro a variável instanciada da classe MailMessage
                smtp.Send(mail);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
