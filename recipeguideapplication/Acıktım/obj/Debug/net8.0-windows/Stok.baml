   M S B A M L   `   `   `  ����  	Acıktım    Acıktım.Stok   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      ���#f0f0f0$��10q�5        � 	Resources  5         CustomRedBrush �� Color  CustomGreenBrush  CustomButtonRedBrush��%�   &	        &	 !     &	 B     �� 5   
   $ #DC143C��61   �� 5   
   $ #32CD32��63   �� 5	   
   $ #FA8072��67   5
      .��5      ��  �� Padding$ 10q�6   ._�5   
   �  � ColumnDefinitions 5      �� 5      $��* �6#   �� 5      $	��200 �6#    � RowDefinitions 5      �� 5      $��* �6    .+�5      � $��0��6   $��0,0,10,0q�6#   ��5      �� 5       �� ImageSource$1 +C:\Users\Berra\Desktop\tarifresim\stok.webp��6!    j� Stretch$ UniformToFill=�5   !    j� 
AlignmentX$ Center=�5   !    j� 
AlignmentY$ Bottom=�65    5      �� 5      $��1* �6'   �� 5       $��3* �6'   �� 5!      $��1* �6'   .+�5$      �� $��1��6   $	��Top=�6/   $��Center=�5%      $��Visible=�5&      $��Disabled=�5'      $��0,10,0,0q�5(      .��5)      &   System.Windows.Controls.DataGrid  -   5        	  Name$	 dgMalzemeler��5)      
   AutoGenerateColumns$
 False��65      
IsReadOnly$
 True��5*         GridLinesVisibility$
 None=�5+         HeadersVisibility$ Column=�6:      	RowHeight$ 50Q�5,         ColumnHeaderHeight$ 50Q�6.   $��0q�6F   
������ 
���   ���5*   1   ��   5.      )  #System.Windows.Controls.DataGridRow �� 
TargetType �� Property �� Value=  7System.Windows.Controls.Primitives.DataGridColumnHeader��%a  '        '  �     7   �� 5/      "  6$   .�50   "   ��  � ��
 �����6?   �� 51   "    � ��$ 16(�6=   �� 52   "    � ��$ 5q�6;   53      �� 54      "  6$   .�55   "   ��  � ��8   6?   �� 56   "    � ��
 �����6?   �� 57   "    � ��$
 Bold"�6?   �� 58   "    � ��$ Center=�6O   59      5:         Columns 5<      0  *System.Windows.Controls.DataGridTextColumn  5=      ,  &System.Windows.Controls.DataGridColumn   Header$ Malzeme Adı��61      Width5  /System.Windows.Controls.DataGridLengthConverter$	 200 5>   P   1  +System.Windows.Controls.DataGridBoundColumn   Binding 61   �� *
MalzemeAdi+6P     5?      $ Toplam Miktar��61   $	 100 5@   R    61   �� *ToplamMiktar+6R     5A      $ Birim��61   $	 100 5B   R    61   �� *MalzemeBirim+6R     5C      $ Birim Fiyat��61   $	 100 5D   P    61   �� *
BirimFiyat+6P   4  .System.Windows.Controls.DataGridTemplateColumn  5E      $ 
İşlemler��65   $	 200 6G      CellTemplate 5F   "   �� 5G   &   .R�5H   *   ��  �� Orientation$ 
Horizontal=�65   $��Center=�6N   .+�5I   .   �� $��Düzenle��65   $	��100��6G   $��40��6S   $	��5,0q�6_   #��[ 5J   5   $��14(�5K   5   $
��Bold"�6C   ��5L   5   �� *DataContext.EditCommand+ �� RelativeSource ��  �� AncestorType DataGrid��5M   5   �� ��5N   2   �� 5O   6    �� 
TargetType" ��6F   .R�5P   :   ��  �� CornerRadius$ 20��6A   #��z��6S   ._�5Q   >   �� $��Center=�6O   $��Center=�5R   O   5S   ;   5T   7   5V   /   �� 5X   .   $	��Sil��65   $��60��6C   $��30��6N   $	��5,0q�6Z   #��[ 5Y   5   $��14(�5Z   5   $
��Bold"�6C   ��5[   5   �� *DataContext.DeleteCommand+ ��  DataGrid��5\   5   �� ��5]   2   �� 5^   6   " ��6F   .R�5_   :   �� $ 10��6A   #��z��6S   ._�5`   >   �� $��Center=�6O   $��Center=�5a   O   5b   ;   5c   7   5e   /   5f   +   5g   '   5i      5k      5l      5m      �� 5o      $��1��6   $��Stretch=�6)   $��10,0,0,0q�6E   .+�5p      �� #��[  6   
�������$��2q�5q      $ 10��6-   $��0,0,0,10q�6?   $��Stretch=�5r      $	��500��65   ._�5s      �� $:��4Sence de stoğunu güncelleme vakti gelmedi mi? 😉��6    $��28(�5t       $
��Bold"�6.   
�������$��Comic Sans MS)�5u   3    �� TextAlignment$ Center=�5v       $
��Wrap=�67   $��Center=�5w       $��Center=�6;    J� Effect 5x      3	  -System.Windows.Media.Effects.DropShadowEffect	  5y       	  Color$ Black��6/     	  
BlurRadius$  8Q�6=   ! 	  ShadowDepth$! 4Q�6L   5{      5|      �� 5~      -   5        $��Malzeme Ekle��5~      $	��150��60   $��50��6<   #��[  5      
�������#��[  5�      $��0q�6G   " � Cursor$
" Hand��5�      $��16(�6'   $
��Bold"�65   $��Center=�5�      $��Center=�66   ��5�      �� 5�      " ��6*   .R�5�      �� $ 25��6%   #��z��67   ._�5�   "   �� $��Center=�63   $��Center=�5�   3   5�      5�      5�      5�      5�      5�      5�      