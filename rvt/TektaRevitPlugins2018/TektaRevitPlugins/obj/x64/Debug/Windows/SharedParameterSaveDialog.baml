   M S B A M L   `   `   `  ����L  HTektaRevitPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:TektaRevitPluginsTektaRevitPlugins  5   	   1    +TektaRevitPlugins.SharedParameterSaveDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:TektaRevitPlugins     5   	     � Title$  SharedParameterSaveDialog��5   	   $	��500��6+   $	��500��68    � 	Resources 5	       �� 
TargetType   myTextBlock �� Property �� Value  myButton&   System.Windows.Controls.DataGrid��%�  &	        &	 �     '      �� 5
   
   " ��6   .�5      ��  � ��$ 10q�6'   �� 5       �@��Padding$ 5q�6(   �� 5       � ��$ 	ExtraBold"�6+   5      �� 5   
   " ��6   .�5      ��  � ��$ 5q�6'   �� 5       � ��$ 3q�6(   5      �� 5   
   "  6   .�5      ��  � ��$ 5q�6'   �� 5       � ��$ 5q�6(   5      5      .��5      ]� $��10q�6   .+�5   
   o� $	��Top=�6   .@�5      m� $��_File��6   .@�5      m� $��_Open��6   5      5      �� 5   
   -   5         ��Name$ sp_names��5      $
��Left=�6$   $��5q�5    7   ��5!      �� 5"      .R�5#      �� ��6    ��  �� Path$
 Name�5$      5&      �� 5'   
   -   5         ��Name$ detailed_info��5'      $��Right=�6,   $��5q�6C   .+�5(      �� $��Guid: ��6   �� 5)      -   5         ��Name$
 guid��5)      ��6&   �� $
 Guid��� 5*      $��Parameter Type: ��6   �� 5+      -   5        $ 	paramType��5+      ��6+   �� $ ParameterType��� 5,      $��Parameter Group: ��6   �� 5-      -   5        $ 
paramGroup��5-      ��6,   �� $ ParameterGroup��� 5.      $��	Binding: ��6   �� 5/      -   5        $	 bnd��5/      ��6%   �� $ Binding��� 50      $��Categories: ��6   �� 51      -   5        $ 
categories��51      {�.  6*   52      54      55      