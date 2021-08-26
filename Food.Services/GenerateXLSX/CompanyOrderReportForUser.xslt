<?xml version="1.0" encoding="UTF-8"?>



<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ms="urn:schemas-microsoft-com:xslt">

  <xsl:template match="ReportUserOrders">
    <html>
      <head>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
        <style>
          table{
          width:100%;
          border-collapse: collapse;
          border: 1px solid black;
          }
          table td{
          border: 1px solid black;
          }
          .td-background td{

          background-color: #f2f2f2;
          }
        </style>
      </head>
      <body>
        <table style="">
          <tr>
            <td colspan ="10">
              <h2>
                Отчет по заказам сотрудника за период
              </h2>
            </td>
          </tr>
          <tr>
            <td>Период</td>
            <td colspan ="9">
              С <xsl:value-of select="concat(ms:format-date(substring-before(StartDate, 'T'), 'dd.MM.yyyy'), ' ', ms:format-time(substring-after(StartDate, 'T'), 'HH:mm'))"></xsl:value-of>
              по <xsl:value-of select="concat(ms:format-date(substring-before(EndDate, 'T'), 'dd.MM.yyyy'), ' ', ms:format-time(substring-after(EndDate, 'T'), 'HH:mm'))"></xsl:value-of>
            </td>
          </tr>
          <tr>
            <td>Заказчик</td>
            <td colspan ="9">
              <xsl:value-of select="User/UserFullName"/>
            </td>
          </tr>
          <tr>
            <td>Сумма заказов за период</td>
            <td colspan ="9">
              <xsl:value-of select="round(100*TotalSumm) div 100"/>
            </td>
          </tr>
          <tr>
            <td colspan="10"></td>
          </tr>
          <tr>
            <td colspan="3">Наименование</td>
            <td>Ед. изм.</td>
            <td>Кол-во</td>
            <td colspan="2">Цена, руб.</td>
            <td>Скидка</td>
            <td colspan="2">Сумма, руб.</td>
          </tr>

          <xsl:for-each select="Orders/OrderModel">
            <tr class="td-background">
              <td>Заказ №</td>
              <td colspan="2">
                <xsl:value-of select="Id"/>
              </td>
              <td colspan="2">Дата заказа</td>
              <td colspan="2">
                <xsl:value-of select="concat(ms:format-date(substring-before(Create, 'T'), 'dd.MM.yyyy'), ' ', ms:format-time(substring-after(Create, 'T'), 'HH:mm'))"/>
              </td>
              <td>Итого:</td>
              <td colspan="2">
                <xsl:value-of select="round(100*TotalSum) div 100"/>
              </td>
            </tr>
            <tr>
              <td>Адрес доставки</td>
              <td colspan="9">
                <xsl:value-of select="OrderInfo/OrderAddress"/>
              </td>
            </tr>
            <tr>
              <td>Исполнитель</td>
              <td colspan="6">
                <xsl:value-of select="Cafe/FullName"/>
              </td>
            <td>Статус</td>
                <td colspan="2">
                  <xsl:variable name="test" select="Status" />
                  <xsl:choose>
                    <xsl:when test="$test = 1">Новый заказ</xsl:when>
                    <xsl:when test="$test = 2">Принят кафе</xsl:when>
                    <xsl:when test="$test = 3">Заказ в процессе доставки</xsl:when>
                    <xsl:when test="$test = 4">Заказ успешно доставлен</xsl:when>
                    <xsl:when test="$test = 5">Заказ отменён</xsl:when>
                    <xsl:when test="$test = 6">Unknown</xsl:when>
                  </xsl:choose>
                </td>
              
            </tr>
            <xsl:for-each select="OrderItems/OrderItemModel">
              <tr>
                <td colspan="3">
                  <xsl:value-of select="DishName"/>
                </td>
                <td>
                  шт.
                </td>
                <td>
                  <xsl:value-of select="DishCount"/>
                </td>
                <td colspan="2">
                  <xsl:value-of select="DishBasePrice"/>
                </td>
                <td>
                  <xsl:if test="Discount > 0">
                    <xsl:value-of select="Discount"/>%
                  </xsl:if>
                </td>
                <td colspan="2">
                  <xsl:value-of select="round(100*TotalPrice) div 100"/>
                </td>
              </tr>
            </xsl:for-each>
            <tr>
              <td colspan="5">Доставка</td>
              <td colspan="2">
                <xsl:value-of select="OrderInfo/DeliverySumm"/>
              </td>
              <td></td>
              <td colspan="2">
                <xsl:value-of select="OrderInfo/DeliverySumm"/>
              </td>
            </tr>
            <tr>
              <td colspan="10"></td>
            </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>